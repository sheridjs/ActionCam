using ICities;
using UnityEngine;

using System.Collections;

namespace BlueJayBird.ActionCam {

	public class ActionCamMod : IUserMod {
		public string Name {
            get { return "Action Cam"; }
        }

		public string Description {
            get { return "A cinematic camera that follows the emergency and disaster recovery action."; }
        }
	}

    public class ActionCamera : CameraExtensionBase {

        public override string Name() {
            return "Action Cam!";
        }

        public override IEnumerator OnStart(ICamera camera) {
            while (true) {
                ushort carid = GetRandomVehicle(Service.PoliceDepartment);

                int camRoutine = FollowVehicle(carid, 10);
                yield return WaitForRoutineToFinish(camRoutine);
            }
        }

    }

}