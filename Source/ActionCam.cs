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
            // TODO: Create a set of camera routines
            // TODO: Create "last resort" road follow camera routine

            // TODO: Randomly select from available camera routines
            routine = FollowCam(id);

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

        // Follow a vehicle from above.
        private IEnumerator FollowCam(ushort id) {
            float duration = 7;
            int fadeRoutine = StartRoutine(FadeInOut(duration));
            yield return WaitForRoutineToFinish(FollowVehicle(id, duration, 50, 45, RandomAngle(45)));
            AbortRoutine(fadeRoutine);
        }

        // TODO: Create "chase cam"

        // TODO: Create "fly-by cam"

        // TODO: Create "orbit cam"

        // TODO: Create "static watch cam" (like Mario Kart) (overhead version?)

    }

}