using UnityEngine;
using Core;
interface ICharacterModule : IModule
{
    BaseCharacter Character { get; }
}