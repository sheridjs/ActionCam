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
            ActionCamRoutine.OrbitCam,
            ActionCamRoutine.StaticCam,
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

                yield return WaitForRoutineToFinish(ChooseRoutine(carid));
            }
        }

        // Choose a random vehicle from a random available service.
        // If no item found, it will return an ID of 0.
        private ushort ChooseVehicle() {
            int index = rn.Next(0, SERVICES.Length);
            ushort id = GetRandomVehicle(SERVICES[index]);

            if (0 == id) {
                // If not found, check the other services for vehicles before returning.
                int nextIndex = (index + 1) % SERVICES.Length;
                while (0 == id && nextIndex != index) {
                    id = GetRandomVehicle(SERVICES[nextIndex]);
                    nextIndex = ++nextIndex % SERVICES.Length;
                }
            }

            return id;
        }

        private int ChooseRoutine(ushort id) {
            IEnumerator routine;

            if (id != 0) {     
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
                    case ActionCamRoutine.OrbitCam:
                        routine = OrbitCam(id);
                        break;
                    case ActionCamRoutine.StaticCam:
                        routine = StaticCam(id);
                        break;
                    default:
                        // We shouldn't hit this case, but just in case..
                        routine = RoadCam();
                        break;
                }
            } else {
                // If no car, just use the road camera.
                routine = RoadCam();
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

        // Calculate a point at a given distance, vertical, and horizontal angle from a given point.
        private void DistantPoint(Vector3 point, float distance, float hAngle, float vAngle, 
                out Vector3 outPoint) {
            // Adjust horizontal angle so behavior matches other Skylines routines where 0 is behind vehicle.
            float hAngleRad = (-90 - hAngle) * Mathf.Deg2Rad;
            float vAngleRad = vAngle * Mathf.Deg2Rad;
            // In Unity, Y is up. Calculate the distant point with XZ as the horizontal plane.
            outPoint.x = point.x + distance * Mathf.Cos(hAngleRad) * Mathf.Cos(vAngleRad);
            outPoint.y = point.y + distance * Mathf.Sin(vAngleRad);
            outPoint.z = point.z + distance * Mathf.Sin(hAngleRad) * Mathf.Cos(vAngleRad);
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
            int camRoutine = StartRoutine(ChaseRoutine(id, duration));
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }

        // Manual camera control routine to chase a car without dodging buildings.
        private IEnumerator ChaseRoutine(ushort id, float duration) {
            float time = 0;
            float x, y, z, angle;
            while (time < duration) {
                GetVehiclePosition(id, out x, out y, out z, out angle);
                SetCameraTarget(x, y, z, 30, 15, angle);
                yield return WaitForNextFrame();
                time += timeDelta;
            }
        }

        // Follow a vehicle from overhead
        private IEnumerator AerialCam(ushort id) {
            float duration = 10;
            int fadeRoutine = StartRoutine(FadeInOut(duration, 2, 2));
            int camRoutine = FollowVehicle(id, duration, 200, RandomAngle(5, 75, 95), RandomAngle(5));
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }

        // Orbit a vehicle
        private IEnumerator OrbitCam(ushort id) {
            float duration = 7;
            int fadeRoutine = StartRoutine(FadeInOut(duration));
            int camRoutine = StartRoutine(OrbitRoutine(id, duration, 10));
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }

        // Manual camera control routine to orbit a moving vehicle
        private IEnumerator OrbitRoutine(ushort id, float duration, float degreesPerSecond) {
            float time = 0;
            float x, y, z, vehicleAngle;
            float angle = RandomAngle(1);
            // TODO Randomize orbit direction
            while (time < duration) {
                GetVehiclePosition(id, out x, out y, out z, out vehicleAngle);
                SetCameraTarget(x, y, z, 75, 60, angle);
                yield return WaitForNextFrame();
                time += timeDelta;
                angle += timeDelta * degreesPerSecond;
            }
        }

        // TODO: Create "fly-by cam"

        // Watch a vehicle from a fixed position.
        private IEnumerator StaticCam(ushort id) {
            float duration = 7;
            int fadeRoutine = StartRoutine(FadeInOut(duration));
            int camRoutine = StartRoutine(StaticRoutine(id, duration));
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }

        // Manual camera routine to watch a vehicle from a fixed point.
        private IEnumerator StaticRoutine(ushort id, float duration) {
            float time = 0;
            Vector3 vehPos = new Vector3();
            float vehAngle;
            // TODO Randomize offset
            float offset = 170;

            // Starting point for vehicle and camera.
            Vector3 camPos = new Vector3();
            GetVehiclePosition(id, out vehPos.x, out vehPos.y, out vehPos.z, out vehAngle);
            DistantPoint(vehPos, 100, vehAngle + offset, 15, out camPos);
            SetPosition(camPos.x, camPos.y, camPos.z, true);

            // Face the vehicle and focus on it.
            while (time < duration) {
                if (time > 0) {
                    GetVehiclePosition(id, out vehPos.x, out vehPos.y, out vehPos.z, out vehAngle);
                }
                FaceTowards(vehPos.x, vehPos.y, vehPos.z);
                SetFocusDistance((vehPos - camPos).magnitude);
                // TODO End routine if car travels away from camera?
                yield return WaitForNextFrame();
                time += timeDelta;
            }
        }

        // Follow a random road
        private IEnumerator RoadCam() {
            float duration = 7;
            int fadeRoutine = StartRoutine(FadeInOut(duration, 2, 2));
            int camRoutine = FollowRoad(GetRandomRoad(), 15, duration, 75, 30, RandomAngle(15));
            yield return WaitForRoutineToFinish(camRoutine);
            AbortRoutine(fadeRoutine);
        }
    }

}