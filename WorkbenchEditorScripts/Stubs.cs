using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Old Unity forces its DLLs to be called Assembly-CSharp
// WurstMod references Assembly-CSharp, H3VR's assembly
// Unity implicitly creates an Assembly-CSharp for every project
// Therefore, Unity thinks it should be able to load H3 classes from Assembly-CSharp...
// Stub out everything we use to silence errors.

namespace FistVR
{
    public class TNH_HoldPoint { }
    public class NavMeshLinkExtension { }
    public class TNH_Manager { }
    public class MainMenuScenePointable { }
    public class FVRViveHand { }
    public class TNH_PointSequence { }
    public class TNH_SafePositionMatrix { }
    public class TNH_SupplyPoint { }
    public class Sosig { }
    public class SosigEnemyTemplate { }
    public class SosigWeapon { }
    public class FVRObject { }
    public class SosigConfigTemplate { }
    public class SosigOutfitConfig { }
    public class SosigLink { }
}
