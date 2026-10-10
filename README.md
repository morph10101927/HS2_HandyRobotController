Honey Select 2 BepInEx plugin, version 1.48. It drives The Handy from the motion in an H scene.

Repository: https://github.com/morph10101927/HS2_HandyRobotController

The plugin reads the animation name, picks the body part that scene uses, and streams that motion to the Handy. Inserted or touching is the bottom of the stroke. Withdrawn or apart is the top. The pose is held back slightly so the picture meets the sleeve.

This build does not use a serial port, COM settings, or T-code.

## Install

Copy `HS2_HandyController.dll` to:

`BepInEx/plugins/HS2_HandyController/HS2_HandyController.dll`

The game needs BepInEx 5. The Handy needs to be on, and you need the device connection key from the Handy app. You do not enter an application key.

<img width="1255" height="927" alt="image" src="https://github.com/user-attachments/assets/51a81d28-070f-40ce-b6d2-87df4cd5251f" />
<img width="417" height="320" alt="image" src="https://github.com/user-attachments/assets/b0680724-aa3d-4a6a-8246-0addd4ae1a4d" />


## Connect

1. Start the game.
2. Press F1 and open Plugin settings.
3. Open HS2_HandyController.
4. Paste the connection key. It is saved and loaded the next time the game starts.
5. Press Connect.

If the Handy was connected the last time you closed the game, it connects again after the game finishes loading.

Settings are stored in `BepInEx/config/hs2robotics.HS2SexRobotController.cfg`.

The game config menu also has Connect Handy, Disconnect Handy, and stroke multiplier buttons. Those work in VR.

## Controls

| Control | Action |
|---|---|
| Connect button, or Left Shift + S | Connect or disconnect the Handy |
| Z, or Stroke Multiplier + | Increase the stroke multiplier |
| X, or Stroke Multiplier - | Decrease the stroke multiplier |

The stroke multiplier starts at 1, which keeps the measured motion. A higher value widens that motion, and the command sent to the Handy stays between 0 and 1.

Invert Stroke flips the direction. Leave it off for the normal Handy direction: 0 is the bottom and 100 is the top.

## Scene delay

The Handy needs the stroke before the moment it should arrive, so the sleeve can already be there when the picture shows that pose. Each motion point is sent about 0.45 seconds ahead. If a point would otherwise reach the device too late, that lead grows, up to another 0.40 seconds. The scene delay is that same amount, usually 0.45 seconds and at most about 0.85 seconds.

While the Handy is connected in an H scene, the plugin reads the characters at their true pose and sends that motion to the device. It then holds the picture back by the scene delay. The sleeve is playing the pose from that short time ago, so the picture and the sleeve meet.

The animation itself keeps its real time. At the start of the next frame the characters are put back on that time, the scene advances, and the picture is held back again after the motion is read. You see the delayed pose. The device follows the undelayed one.

The hold is not applied while a character is changing clips, and it lets go if a clip stops advancing. Choosing another scene and starting a scene with the scroll wheel still work. When the Handy is disconnected, the scene plays at normal time.

## How a scene is measured

The animation name selects one contact.

Penis scenes measure that contact along the penis:

| Contact | What is measured |
|---|---|
| Mouth | Mouth on the penis |
| Left hand or right hand | That hand on the penis |
| Breasts | Breasts on the penis |
| Vagina | Vagina |
| Anus | Anus |
| Thighs | Thighs |
| Foot | Foot on the penis |

Scenes without a penis measure the two parts that act uses. Together is the bottom of the stroke, and apart is the top.

| Contact | What is measured |
|---|---|
| Kiss | The two mouths |
| Hand on breasts | A hand against the breasts |
| Fingers | A hand at the vagina |
| Mouth at vagina | Mouth at the vagina |
| Mouth at anus | Mouth at the anus |
| Hand on hip | A hand against the hip |
| Vibrator | The vibrator at the anus |

A swap scene uses the second woman.

## Service

| Animation | Contact |
|---|---|
| Blowjob | Mouth |
| Handjob | Left hand |
| Glans Tease | Mouth |
| Boobjob | Breasts |
| Licking Boobjob | Breasts |
| Sucking Boobjob | Breasts |
| Exhausted Handjob | Right hand |
| Exhausted Blowjob | Mouth |
| Standing Handjob | Right hand |
| No-Hands Tip Licking | Mouth |
| No-Hands Blowjob | Mouth |
| Deepthroat | Mouth |
| Standing Boobjob | Breasts |
| Stand. Lick. Boobjob | Breasts |
| Restrained Blowjob | Mouth |
| Forced Handjob | Right hand |
| Irrumatio | Mouth |
| Chair Handjob | Left hand |
| Sit. No-Hand Blowjob | Mouth |
| Sitting Boobjob | Breasts |
| Sitting Licking Boobjob | Breasts |
| Wall-Trapped Blowjob | Mouth |
| Crouching Blowjob | Mouth |
| Desk Handjob | Right hand |
| Behind Handjob | Left hand |
| Sleeping Boobjob | Breasts |
| Wall Irramatio | Mouth |
| Chair Irramatio | Mouth |
| Pet Blowjob | Mouth |
| Forceful BJ | Mouth |
| Sitting Handjob | Left hand |
| Titjob | Breasts |
| Tip Lick (No Hands) | Mouth |
| Sitting Titjob | Breasts |
| Stockade Femdom BJ | Mouth |

## Foreplay

| Animation | Contact |
|---|---|
| Kiss | Kiss |
| Standing Kiss | Kiss |
| Breast Play | Hand on breasts |
| Breast Massage (Behind) | Hand on breasts |
| Squatting Breast Massage | Hand on breasts |
| Kiss Grope | Hand on breasts |
| Fingering | Fingers |
| Reclining Masturbation | Fingers |
| All-Fours Foreplay | Fingers |
| Cunnilingus | Mouth at vagina |
| Standing Cunnilingus | Mouth at vagina |
| Face Sit Cunnilingus | Mouth at vagina |
| Rimjob | Mouth at anus |
| Anal Vibrator | Vibrator |
| Desk Spank | Hand on hip |
| Chair Spank | Hand on hip |

## Insert

| Animation | Contact |
|---|---|
| Missionary | Vagina |
| Breast Grope Missionary | Vagina |
| Doggy | Vagina |
| Kneeling Behind | Vagina |
| Spooning | Vagina |
| Cowgirl | Vagina |
| Chest Grope Cowgirl | Vagina |
| Reverse Cowgirl | Vagina |
| Piledriver Missionary | Vagina |
| Bent Missionary | Vagina |
| Double Decker | Vagina |
| Anal Missionary | Anus |
| Anal Doggy | Anus |
| Floor Bondage Miss. | Vagina |
| Forced Missionary | Vagina |
| Standing | Vagina |
| Standing Behind | Vagina |
| Thrust Behind | Vagina |
| Lifting | Vagina |
| Reverse Lifting | Vagina |
| Thighjob | Thighs |
| Stockade | Vagina |
| Wall-Facing Behind | Vagina |
| Wall-Facing Anal | Anus |
| Chair Sitting Behind | Vagina |
| Anal Doggy on Chair | Anus |
| Desk Missionary | Vagina |
| Desk on Side | Vagina |
| Desk Doggy | Anus |
| Against Counter Behind | Vagina |
| Wall-Trapped Doggy | Vagina |
| Crouch Insertion | Vagina |
| Delivery Table Insert | Vagina |
| Hanging | Vagina |
| Tied Up Insertion | Vagina |
| Lying Doggystyle | Vagina |
| Anal Piledriver Miss. | Anus |
| Face to Face Sitting | Vagina |
| Chest Grope Sit Behind | Vagina |
| Sitting Hugging | Vagina |
| Restrained Standing | Vagina |
| Clinging Lifting | Vagina |
| Wall-Pressed Behind | Vagina |
| Pet Sex | Vagina |
| Mating Press | Vagina |
| Domination Reverse Cowgirl | Vagina |
| Stockade Femdom Sex | Vagina |
| Side | Vagina |
| Standing Doggy | Vagina |
| Standing Missionary | Vagina |
| Standing Wall Doggy | Vagina |
| Table Missionary | Vagina |
| Missionary Press | Vagina |
| Counter Doggy | Vagina |
| Squatting Sex | Vagina |
| Suspended Sex | Vagina |
| Standing Tied | Vagina |
| Standing Squeeze | Vagina |

## Woman-led

| Animation | Contact |
|---|---|
| Nipple Licking Blowjob | Right hand |
| Rimjob + Handjob | Right hand |
| Standing Footjob | Foot |
| Sitting Footjob | Foot |
| All Fours Handjob | Right hand |
| Chair Restraint Footjob | Foot |
| Reverse Piledriver | Vagina |
| Cowgirl Intercrural | Thighs |
| Handjob Intercrural | Thighs |
| Chair Intercrural | Thighs |
| Piledriver Rev. Cowgirl | Vagina |
| Anal Reverse Cowgirl | Anus |
| Male Restrained Stand | Vagina |
| Chair Restraint Sitting | Vagina |

Cowgirl, Reverse Cowgirl, and Standing in this category use the same contacts as the insert list.

## Group and special

| Animation | Contact |
|---|---|
| W Cowgirl | Vagina |
| W Cowgirl (swap) | Second woman's vagina |
| W Blowjob | Mouth |
| W Blowjob (swap) | Second woman's mouth |
| Intercrural Sandwich | Thighs |
| Interc. Sandwich (swap) | Second woman's thighs |
| Insertion + Fingering | Vagina |
| Insert. + Fing. (swap) | Second woman's vagina |
| W Handjob Licking | Second woman's right hand |
| W Handjob Lick. (swap) | Right hand |
| Sitting+Cunilingus | Vagina |
| Sit+Cunilingus (swap) | Second woman's vagina |
| 69 | Mouth |

## Build

The project file is `HS2_SexRobotController/HS2_SexRobotController.csproj`. A Release build writes `HS2_HandyController.dll`.
