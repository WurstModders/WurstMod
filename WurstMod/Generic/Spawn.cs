using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.Generic
{
    public class Spawn : ComponentProxy
    {
        void OnDrawGizmos()
        {
            Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0.8f, 0.5f), Vector3.zero, 0.25f, transform);
        }

        protected override bool InitializeComponent()
        {
            ObjectReferences.CameraRig.transform.position = transform.position;
			if (ManagerSingleton<GM>.Instance != null && GM.CurrentSceneSettings != null)
				GM.CurrentSceneSettings.DeathResetPoint = transform;
            return true;
        }
    }
}
