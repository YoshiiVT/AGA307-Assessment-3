using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    protected static CharacterManager _PLAYER { get { return CharacterManager.instance; } }
    protected static MonsterManager _MONSTER { get { return MonsterManager.instance; } }
}
