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

                int fadeRoutine = StartRoutine(FadeInOut(7, 0.5f, 1));
                int camRoutine = ChooseRoutine(carid);
                yield return WaitForRoutineToFinish(camRoutine);
                AbortRoutine(fadeRoutine);
            }
        }

        // Choose a random vehicle from a random available service.
        // If no item found, it will return an ID of 0.
        private ushort ChooseVehicle() {
            Service service = SERVICES[rn.Next(0, SERVICES.Length)];
            return GetRandomVehicle(service);
        }

        private int ChooseRoutine(ushort id) {
            // TODO: Create a set of camera routines
            // TODO: Randomly select from available camera routines
            // TODO: Create "last resort" road follow camera routine
            return FollowVehicle(id, 7);
        }

        // Combine fade in and fade out routines over a given total duration
        private IEnumerator FadeInOut(float totalDuration, float fadeInDuration = 1.0f, float fadeOutduration = 0.5f) {
            yield return WaitForRoutineToFinish(FadeIn(fadeInDuration));
            yield return Wait(totalDuration - fadeInDuration - fadeOutduration);
            yield return WaitForRoutineToFinish(FadeOut(fadeOutduration));
        }

        // TODO: Add some variety to "follow cam"

        // TODO: Create "chase cam"

        // TODO: Create "fly-by cam"

        // TODO: Create "orbit cam"

        // TODO: Create "static watch cam" (like Mario Kart) (overhead version?)

    }

}