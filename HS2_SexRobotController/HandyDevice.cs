using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HS2_HandyController
{
    public class HandyDevice
    {
        public const string BaseUrl = "https://www.handyfeeling.com/api/handy-rest/v3";

        // Public application id from the Handy developer portal. This is not the Application Key.
        const string ApplicationId = "2zDLJ8HZ2AMYW_OFFF1qN_.aUm1HvtHg";

        public bool IsConnected { get; private set; }
        public int LastX { get { return lastQueuedX; } }
        public int Queued { get { return pending.Count; } }
        public bool StreamPlaying { get { return playing; } }
        public long OffsetMs { get { return clockOffset; } }

        readonly Action<string> log;
        readonly List<MotionSample> pending = new List<MotionSample>();
        int session;
        bool haveSample;
        float lastSampleAt = -1f;
        int lastQueuedX = -1;
        float nextSendAt = -10f;
        bool playing;
        long clockOffset;
        long extraLeadMs;
        long epochWall;
        int tailIndex;
        long lastPointT = -1;
        int streamGeneration;
        int appliedGeneration;
        float lastMoveLogAt = -10f;

        public const int PlaybackDelayMs = 450;
        const float BatchGapSeconds = 0.25f;
        const long MaxExtraLeadMs = 400;

        public float VisualDelaySeconds
        {
            get { return (PlaybackDelayMs + extraLeadMs) / 1000f; }
        }

        public HandyDevice(Action<string> log)
        {
            this.log = log;
        }

        public IEnumerator Connect(string connectionKey, Action<string> setStatus, Func<float> intervalSource)
        {
            int op = BeginSession();
            connectionKey = Clean(connectionKey);

            if (!IsConnectionKeyValid(connectionKey))
            {
                setStatus("Disconnected - enter the connection key in plugin settings");
                Log("Handy connection key is missing or invalid. Enter it in the Handy settings. It is saved for the next time the game starts.");
                yield break;
            }

            setStatus("Connecting...");

            WebResult connected = new WebResult();
            yield return Request("GET", "/connected", null, connectionKey, connected);
            if (op != session)
            {
                yield break;
            }

            if (!connected.IsOk || !HasFlag(connected.Body, "connected"))
            {
                setStatus(FailureStatus(connected, "Handy is not online"));
                Log("Handy connect failed: " + Describe(connected) + " " + OneLine(connected.Body));
                yield break;
            }

            WebResult stop = new WebResult();
            yield return Request("PUT", "/hamp/stop", "{}", connectionKey, stop);
            if (op != session)
            {
                yield break;
            }

            WebResult streamStop = new WebResult();
            yield return Request("PUT", "/hsp/stop", "{}", connectionKey, streamStop);
            if (op != session)
            {
                yield break;
            }

            float settle = Time.realtimeSinceStartup + 0.6f;
            while (op == session && Time.realtimeSinceStartup < settle)
            {
                yield return null;
            }

            if (op != session)
            {
                yield break;
            }

            bool synced = false;
            yield return SyncClock(op, connectionKey, value => synced = value);
            if (op != session)
            {
                yield break;
            }

            if (!synced)
            {
                setStatus("Disconnected - could not reach Handy");
                Log("Handy server time sync failed.");
                yield break;
            }

            WebResult mode = new WebResult();
            yield return Request("PUT", "/mode", "{\"mode\":4}", connectionKey, mode);
            if (op != session)
            {
                yield break;
            }

            if (!mode.IsOk || HasError(mode.Body) || IsNegativeResult(mode.Body))
            {
                setStatus(FailureStatus(mode, "could not start streaming"));
                Log("Handy stream mode failed: " + Describe(mode) + " " + OneLine(mode.Body));
                yield break;
            }

            WebResult setup = new WebResult();
            yield return Request("PUT", "/hsp/setup", "{}", connectionKey, setup);
            if (op != session)
            {
                yield break;
            }

            if (!setup.IsOk || HasError(setup.Body))
            {
                setStatus(FailureStatus(setup, "could not start streaming"));
                Log("Handy stream setup failed: " + Describe(setup) + " " + OneLine(setup.Body));
                yield break;
            }

            WebResult stroke = new WebResult();
            yield return Request("PUT", "/slider/stroke", "{\"min\":0,\"max\":1}", connectionKey, stroke);
            if (op != session)
            {
                yield break;
            }

            if (!stroke.IsOk || HasError(stroke.Body))
            {
                Log("Handy stroke zone update failed: " + Describe(stroke) + " " + OneLine(stroke.Body));
            }
            else
            {
                Log("Handy stroke zone set to the full slide.");
            }

            IsConnected = true;
            setStatus("Connected");
            Log("Handy connected. Motion is sent as a smooth point curve.");

            while (op == session)
            {
                yield return Pump(op, connectionKey, setStatus);
                float waitUntil = Time.realtimeSinceStartup + 0.02f;
                while (op == session && Time.realtimeSinceStartup < waitUntil)
                {
                    yield return null;
                }
            }
        }

        public IEnumerator Disconnect(string connectionKey, Action<string> setStatus)
        {
            bool wasConnected = IsConnected;
            BeginSession();
            setStatus("Disconnected");

            connectionKey = Clean(connectionKey);
            if (!wasConnected || !IsConnectionKeyValid(connectionKey))
            {
                Log("Handy disconnected.");
                yield break;
            }

            WebResult stop = new WebResult();
            yield return Request("PUT", "/hsp/stop", "{}", connectionKey, stop);
            Log(stop.IsOk && !HasError(stop.Body) ? "Handy stopped." : "Handy stop failed: " + Describe(stop) + " " + OneLine(stop.Body));
        }

        public void ResetStream()
        {
            pending.Clear();
            haveSample = false;
            lastQueuedX = -1;
            streamGeneration++;
        }

        public void ReportMotion(float xp)
        {
            if (!IsConnected)
            {
                return;
            }

            int x = (int)Math.Round(xp);
            if (x < 0)
            {
                x = 0;
            }
            else if (x > 100)
            {
                x = 100;
            }

            float now = Time.realtimeSinceStartup;
            lastSampleAt = now;
            haveSample = true;
            if (x == lastQueuedX)
            {
                return;
            }

            pending.Add(new MotionSample { UnityTime = now, X = x });
            lastQueuedX = x;
            if (pending.Count > 180)
            {
                pending.RemoveAt(0);
            }
        }

        IEnumerator SyncClock(int op, string connectionKey, Action<bool> done)
        {
            long offsetSum = 0;
            int samples = 0;
            int attempts = 0;
            while (samples < 4 && attempts < 10)
            {
                attempts++;
                long send = NowMs();
                WebResult result = new WebResult();
                yield return Request("GET", "/servertime", null, connectionKey, result);
                long receive = NowMs();
                if (op != session)
                {
                    yield break;
                }

                long serverTime;
                if (!result.IsOk || !TryReadLong(result.Body, "server_time", out serverTime))
                {
                    continue;
                }

                long rtd = receive - send;
                if (rtd < 0 || rtd > 400)
                {
                    Log("Handy clock sample skipped, round trip " + rtd.ToString(CultureInfo.InvariantCulture) + " ms.");
                    continue;
                }

                offsetSum += serverTime + (rtd / 2) - receive;
                samples++;
            }

            if (samples == 0)
            {
                done(false);
                yield break;
            }

            clockOffset = offsetSum / samples;
            Log("Handy clock synced, offset " + clockOffset.ToString(CultureInfo.InvariantCulture) + " ms.");
            done(true);
        }

        IEnumerator Pump(int op, string connectionKey, Action<string> setStatus)
        {
            if (appliedGeneration != streamGeneration)
            {
                int gen = streamGeneration;
                appliedGeneration = gen;
                bool wasPlaying = playing;
                playing = false;
                tailIndex = 0;
                lastPointT = -1;
                if (wasPlaying)
                {
                    WebResult reset = new WebResult();
                    yield return Request("PUT", "/hsp/stop", "{}", connectionKey, reset);
                    if (op != session || gen != streamGeneration)
                    {
                        yield break;
                    }
                }
            }

            float now = Time.realtimeSinceStartup;
            if (!haveSample || pending.Count == 0)
            {
                if (playing && now - lastSampleAt > 2f)
                {
                    WebResult stop = new WebResult();
                    yield return Request("PUT", "/hsp/stop", "{}", connectionKey, stop);
                    if (op != session)
                    {
                        yield break;
                    }

                    playing = false;
                }

                yield break;
            }

            if (now < nextSendAt)
            {
                yield break;
            }

            nextSendAt = now + BatchGapSeconds;
            int take = pending.Count;
            if (take > 80)
            {
                take = 80;
            }

            List<MotionSample> batch = pending.GetRange(0, take);
            pending.RemoveRange(0, take);
            long nowWall = NowMs();
            long earliest = long.MaxValue;
            for (int i = 0; i < batch.Count; i++)
            {
                long wall = SampleWall(batch[i], now, nowWall);
                if (wall < earliest)
                {
                    earliest = wall;
                }
            }

            if (earliest < nowWall + 100)
            {
                long need = (nowWall + 100) - earliest;
                if (extraLeadMs + need > MaxExtraLeadMs)
                {
                    need = MaxExtraLeadMs - extraLeadMs;
                }

                if (need > 0)
                {
                    extraLeadMs += need;
                }
            }

            if (!playing)
            {
                epochWall = nowWall;
                lastPointT = -1;
                tailIndex = 0;
            }

            long draftT;
            int written;
            long span;
            string points = PointsJson(batch, now, nowWall, out draftT, out written, out span);
            if (points == null)
            {
                yield break;
            }

            int prospectiveTail = tailIndex + written;
            WebResult result = new WebResult();
            if (!playing)
            {
                long serverTime = nowWall + clockOffset;
                string body = "{\"start_time\":0,\"server_time\":" + serverTime.ToString(CultureInfo.InvariantCulture) + ",\"pause_on_starving\":false,\"add\":{\"flush\":true,\"tail_point_stream_index\":" + prospectiveTail.ToString(CultureInfo.InvariantCulture) + ",\"points\":" + points + "}}";
                yield return Request("PUT", "/hsp/play", body, connectionKey, result);
            }
            else
            {
                string body = "{\"flush\":false,\"tail_point_stream_index\":" + prospectiveTail.ToString(CultureInfo.InvariantCulture) + ",\"points\":" + points + "}";
                yield return Request("PUT", "/hsp/add", body, connectionKey, result);
            }

            if (op != session)
            {
                yield break;
            }

            if (result.Code == 429)
            {
                pending.InsertRange(0, batch);
                nextSendAt = Time.realtimeSinceStartup + 0.5f;
                Log("Handy rate limit. Waiting before the next batch.");
                yield break;
            }

            if (!result.IsOk || HasError(result.Body))
            {
                pending.InsertRange(0, batch);
                nextSendAt = Time.realtimeSinceStartup + 0.4f;
                Log("Handy curve update failed: " + Describe(result) + " " + OneLine(result.Body));
                if (IsDeviceGone(result))
                {
                    playing = false;
                    IsConnected = false;
                    setStatus(FailureStatus(result, "Handy is not online"));
                }

                yield break;
            }

            lastPointT = draftT;
            tailIndex = prospectiveTail;
            playing = true;
            if (Time.realtimeSinceStartup - lastMoveLogAt > 2f)
            {
                lastMoveLogAt = Time.realtimeSinceStartup;
                Log("Handy curve batch " + written.ToString(CultureInfo.InvariantCulture) + " points, span " + span.ToString(CultureInfo.InvariantCulture) + "ms, last " + batch[batch.Count - 1].X.ToString(CultureInfo.InvariantCulture) + ".");
            }
        }

        string PointsJson(List<MotionSample> batch, float unityNow, long nowWall, out long endT, out int written, out long span)
        {
            endT = lastPointT;
            written = 0;
            span = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            long cursor = lastPointT;
            long firstT = -1;
            for (int i = 0; i < batch.Count; i++)
            {
                long wall = SampleWall(batch[i], unityNow, nowWall);
                long t = wall - epochWall;
                if (t <= cursor)
                {
                    continue;
                }

                if (firstT < 0)
                {
                    firstT = t;
                }

                if (written > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"t\":").Append(t.ToString(CultureInfo.InvariantCulture));
                sb.Append(",\"x\":").Append(batch[i].X.ToString(CultureInfo.InvariantCulture));
                sb.Append('}');
                cursor = t;
                written++;
            }

            if (written == 0)
            {
                return null;
            }

            endT = cursor;
            span = cursor - firstT;
            sb.Append(']');
            return sb.ToString();
        }

        long SampleWall(MotionSample sample, float unityNow, long nowWall)
        {
            long age = (long)((unityNow - sample.UnityTime) * 1000f);
            if (age < 0)
            {
                age = 0;
            }

            return nowWall - age + PlaybackDelayMs + extraLeadMs;
        }

        static long NowMs()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        int BeginSession()
        {
            session++;
            IsConnected = false;
            playing = false;
            haveSample = false;
            lastSampleAt = -1f;
            lastQueuedX = -1;
            nextSendAt = -10f;
            tailIndex = 0;
            lastPointT = -1;
            extraLeadMs = 0;
            streamGeneration = 0;
            appliedGeneration = 0;
            lastMoveLogAt = -10f;
            pending.Clear();
            return session;
        }

        IEnumerator Request(string method, string path, string json, string connectionKey, WebResult result)
        {
            UnityWebRequest req = new UnityWebRequest(BaseUrl + path, method);
            if (!string.IsNullOrEmpty(json))
            {
                UploadHandlerRaw upload = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                upload.contentType = "application/json";
                req.uploadHandler = upload;
            }

            req.downloadHandler = new DownloadHandlerBuffer();
            req.chunkedTransfer = false;
            req.timeout = 8;
            req.SetRequestHeader("Accept", "application/json");
            req.SetRequestHeader("X-Api-Key", ApplicationId);
            req.SetRequestHeader("X-Connection-Key", connectionKey);
            if (!string.IsNullOrEmpty(json))
            {
                req.SetRequestHeader("Content-Type", "application/json");
            }

            yield return req.SendWebRequest();

            result.Code = req.responseCode;
            result.Body = req.downloadHandler == null ? "" : req.downloadHandler.text;
            result.Error = req.error;
            result.TransportFailed = req.isNetworkError;
            req.Dispose();
        }

        static bool TryReadLong(string json, string key, out long value)
        {
            value = 0;
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
            {
                return false;
            }

            string token = "\"" + key + "\"";
            int keyIndex = json.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return false;
            }

            int colon = json.IndexOf(':', keyIndex + token.Length);
            if (colon < 0)
            {
                return false;
            }

            int start = colon + 1;
            while (start < json.Length && char.IsWhiteSpace(json[start]))
            {
                start++;
            }

            int end = start;
            while (end < json.Length && char.IsDigit(json[end]))
            {
                end++;
            }

            if (end == start)
            {
                return false;
            }

            return long.TryParse(json.Substring(start, end - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        static bool TryReadNumber(string json, string key, out float value)
        {
            value = 0f;
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
            {
                return false;
            }

            string compact = json.ToLowerInvariant();
            string token = "\"" + key.ToLowerInvariant() + "\"";
            int keyIndex = compact.IndexOf(token, StringComparison.Ordinal);
            if (keyIndex < 0)
            {
                return false;
            }

            int colon = compact.IndexOf(':', keyIndex + token.Length);
            if (colon < 0)
            {
                return false;
            }

            int start = colon + 1;
            while (start < compact.Length && char.IsWhiteSpace(compact[start]))
            {
                start++;
            }

            int end = start;
            if (end < compact.Length && compact[end] == '-')
            {
                end++;
            }

            while (end < compact.Length && (char.IsDigit(compact[end]) || compact[end] == '.'))
            {
                end++;
            }

            if (end == start)
            {
                return false;
            }

            return float.TryParse(compact.Substring(start, end - start), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        static string FailureStatus(WebResult result, string fallback)
        {
            if (result.Code == 400 || result.Code == 401 || result.Code == 403)
            {
                return "Disconnected - check the connection key";
            }

            if (result.Code == 502 || result.Code == 504 || (result.Body != null && result.Body.Contains("DeviceNotConnected")))
            {
                return "Disconnected - Handy is not online";
            }

            if (result.TransportFailed)
            {
                return "Disconnected - network error";
            }

            return "Disconnected - " + fallback;
        }

        static string Describe(WebResult result)
        {
            string message = ReadJsonString(result.Body, "message");
            if (!string.IsNullOrEmpty(message))
            {
                return result.Code + " " + message;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                return result.Code + " " + result.Error;
            }

            return result.Code.ToString();
        }

        static bool HasFlag(string json, string name)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            string compact = json.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").ToLowerInvariant();
            return compact.Contains("\"" + name + "\":true");
        }

        static bool IsDeviceGone(WebResult result)
        {
            if (result.Code == 502 || result.Code == 504)
            {
                return true;
            }

            return result.Body != null && result.Body.Contains("DeviceNotConnected");
        }

        static bool IsNegativeResult(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            string compact = json.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").ToLowerInvariant();
            return compact.Contains("\"result\":-");
        }

        static bool HasError(string json)
        {
            return !string.IsNullOrEmpty(json) && json.ToLowerInvariant().Contains("\"error\"");
        }

        static string ReadJsonString(string json, string key)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
            {
                return null;
            }

            string token = "\"" + key + "\"";
            int keyIndex = json.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return null;
            }

            int colon = json.IndexOf(':', keyIndex + token.Length);
            if (colon < 0)
            {
                return null;
            }

            int start = json.IndexOf('"', colon + 1);
            if (start < 0)
            {
                return null;
            }

            int end = json.IndexOf('"', start + 1);
            if (end < 0)
            {
                return null;
            }

            return json.Substring(start + 1, end - start - 1);
        }

        static string OneLine(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return value.Replace("\r", " ").Replace("\n", " ").Trim();
        }

        static string Clean(string value)
        {
            if (value == null)
            {
                return "";
            }

            return value.Trim();
        }

        public static bool IsConnectionKeyValid(string connectionKey)
        {
            if (string.IsNullOrEmpty(connectionKey) || connectionKey.Length < 5 || connectionKey.Length > 64)
            {
                return false;
            }

            for (int i = 0; i < connectionKey.Length; i++)
            {
                char c = connectionKey[i];
                bool alpha = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                if (!alpha && !digit)
                {
                    return false;
                }
            }

            return true;
        }

        void Log(string message)
        {
            if (log != null)
            {
                log(message);
            }
        }

        class MotionSample
        {
            public float UnityTime;
            public int X;
        }

        class WebResult
        {
            public long Code;
            public string Body;
            public string Error;
            public bool TransportFailed;

            public bool IsOk
            {
                get { return !TransportFailed && Code >= 200 && Code < 300; }
            }
        }
    }
}
