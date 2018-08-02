using ICities;
using UnityEngine;

using System;
using System.Collections;

namespace BlueJayBird.ActionCam {

	public class ActionCam : IUserMod {

        public static string NAME = "Action Cam!";

		public string Name {
            get { return ActionCam.NAME; }
        }

		public string Description {
            get { return "A cinematic camera that follows the emergency and disaster recovery action."; }
        }
	}

    public enum ActionCamRoutine {
        AerialCam,
        ChaseCam,
        FlyByCam,
        FollowCam,
        OrbitCam,
        StaticCam,
    }

    public class ActionCamera : CameraExtensionBase {

        // Debug log marker
        private static string LOG_MARKER = "[ActionCam] ";

        // Available services.
        private static Service[] SERVICES = new Service[] { 
            Service.FireDepartment, 
            Service.PoliceDepartment,
            Service.HealthCare,
            Service.Disaster
        };

        private static ActionCamRoutine[] CAM_ROUTINES = new ActionCamRoutine[] {
            ActionCamRoutine.AerialCam,
            ActionCamRoutine.ChaseCam,
            ActionCamRoutine.FollowCam,
        };
        
        // Random number generator.
        private System.Random rn = new System.Random();

        public override string Name() {
            return ActionCam.NAME;
        }

        public override IEnumerator OnStart(ICamera camera) {
            Debug.Log(LOG_MARKER + "camera started");
            // TODO: VehicleManager manager = Singleton<VehicleManager>.instance;

            while (true) {                
                ushort carid = ChooseVehicle();
                Debug.Log(LOG_MARKER + "vehicle id = " + carid);
                if (0 == carid) {
                    // TODO: Look for a better way to handle cars not found.
                    yield return WaitForNextFrame();
                    continue;
                }

                yield return WaitForRoutineToFinish(ChooseRoutine(carid));
            }
        }

        // Choose a random vehicle from a random available service.
        // If no item found, it will return an ID of 0.
        private ushort ChooseVehicle() {
            Service service = SERVICES[rn.Next(0, SERVICES.Length)];
            return GetRandomVehicle(service);
        }

        private int ChooseRoutine(ushort id) {
            IEnumerator routine;
            
            // Randomly select from available camera routines
            ActionCamRoutine routineType = CAM_ROUTINES[rn.Next(0, CAM_ROUTINES.Length)];
            switch (routineType)
            {
                case ActionCamRoutine.AerialCam:
                    routine = AerialCam(id);
                    break;
                case ActionCamRoutine.ChaseCam:
                    routine = ChaseCam(id);
                    break;
                case ActionCamRoutine.FollowCam:
                    routine = FollowCam(id);
                    break;
                default:
                    // TODO: Create "last resort" road follow camera routine
                    routine = FollowCam(id);
                    break;
            }
            
            return StartRoutine(routine);
        }

        // Combine fade in and fade out routines over a given total duration
        private IEnumerator FadeInOut(float totalDuration, float fadeInDuration = 0.5f, float fadeOutduration = 1.0f) {
            yield return WaitForRoutineToFinish(FadeIn(fadeInDuration));
            yield return Wait(totalDuration - fadeInDuration - fadeOutduration);
            yield return WaitForRoutineToFinish(FadeOut(fadeOutduration));
        }

        // Generate a random angle between the start and end angles at the given degree step
        // ex: a step of 90 will generate a random angle of 0, 90, 180, or 270.
        private float RandomAngle(float step, float start = 0f, float end = 360f) {
            int angle = rn.Next(Convert.ToInt32(start/step), Convert.ToInt32(end/step));
            return angle * step;
        }

        // Follow a vehicle from above at an angle.
        private IEnumerator FollowCam(ushort id) {
            float duration = 7;
            int fadeRoutine = StartRoutine(FadeInOut(duration));
            int camRoutine = FollowVehicle(id, duration, 50, 45, RandomAngle(45));
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }

        // Follow a vehicle from behind
        private IEnumerator ChaseCam(ushort id) {
            float duration = 7;
            int fadeRoutine = StartRoutine(FadeInOut(duration));
            // TODO: Fix bumpiness
            int camRoutine = FollowVehicle(id, duration, 40, 15, 0, true);
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }

        // Follow a vehicle from overhead
        private IEnumerator AerialCam(ushort id) {
            float duration = 10;
            int fadeRoutine = StartRoutine(FadeInOut(duration, 2, 2));
            int camRoutine = FollowVehicle(id, duration, 200, RandomAngle(5, 75, 95), RandomAngle(5));
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }

        // TODO: Create "fly-by cam"

        // TODO: Create "orbit cam"

        // TODO: Create "static watch cam" (like Mario Kart) (overhead version?)

    }

}