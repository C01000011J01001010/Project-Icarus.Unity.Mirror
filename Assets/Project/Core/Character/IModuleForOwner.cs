using UnityEngine;
using CoreEngine;
interface ICharacterModule : IModule
{
    BaseCharacter Character { get; }
}