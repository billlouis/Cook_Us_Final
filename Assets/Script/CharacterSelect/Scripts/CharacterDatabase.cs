using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[CreateAssetMenu(fileName= "New Character", menuName = "Characters/Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private Character[] characters = new Character[0];

    public Character[] GetAllCharacters() => characters;

    public Character GetCharacterById(int id){
        foreach(var character in characters){
            if(character.Id == id){
                return character;
            }
        }

        return null;
    }

    public bool IsValidCharacterId(int id){
        return characters.Any(x => x.Id == id);
    }
}
