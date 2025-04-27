using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    protected static CharacterManager _PLAYER { get { return CharacterManager.instance; } }
    protected static MonsterManager _MONSTER { get { return MonsterManager.instance; } }
    protected static FuseboxManager _FM { get { return FuseboxManager.instance; }}
    protected static GameManager _GM { get { return GameManager.instance; }}
    protected static FlashlightManager _FLM { get { return FlashlightManager.instance; } }
}
