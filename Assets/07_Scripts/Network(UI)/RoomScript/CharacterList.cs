using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterList : MonoBehaviour
{
    public CharacterSo[] characters;
    public GameObject kartPrefab;
    public RawImage characterImage;
    public CharacterSo characterSo;
    public List<GameObject> characterListPrefab;
    private string characterName;
    private int Index = 0;
    private void Awake()
    {
        characters = Resources.LoadAll<CharacterSo>("Character");
        characterListPrefab = new List<GameObject>();
                
        SetCharacterResources();
    }

    public void SetCharacterResources()
    {
        foreach (var character in characters)
        {
            if (character.characterName == "Airi" || character.characterName == "Lena")
            {
                GameObject playerChar = Instantiate(character.characterPrefab, Vector3.zero, Quaternion.Euler(-90, 0, -90));
                characterListPrefab.Add(playerChar);
                playerChar.gameObject.SetActive(false);
            }
            else if (character.characterName == "Bazzi" || character.characterName == "Dao" || character.characterName == "Kephi")
            {
                GameObject playerChar = Instantiate(character.characterPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
                characterListPrefab.Add(playerChar);
                playerChar.gameObject.SetActive(false);
            }
        }
        StartCoroutine(KartandCharacterCor());
    }

    public void CharacterChangeNextBtn()
    {
        characterListPrefab[Index].gameObject.SetActive(false);
        Index = (Index + 1) % characterListPrefab.Count;
        characterListPrefab[Index].gameObject.SetActive(true);
    }
    public void PreviousCharacterBtn()
    {
        characterListPrefab[Index].gameObject.SetActive(false);
        Index = (Index - 1 + characterListPrefab.Count) % characterListPrefab.Count;
        characterListPrefab[Index].gameObject.SetActive(true);
    }
    public CharacterSo SelectedCharacter()
    {
        if (SceneCont.Instance != null)
        {
            SceneCont.Instance.SelectedCharacter = characters[Index];
            var saveTask = FirebaseDBManager.Instance.DbRef.Child("users")
                .Child(FirebaseDBManager.Instance.User.UserId)
                .Child("SelectedCharacter")
                .SetValueAsync(characters[Index].characterName);
        }
        return characters[Index];
    }
    IEnumerator KartandCharacterCor()
    {
        if (!string.IsNullOrEmpty("SelectedCharacter"))
        {            
            var characterTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("SelectedCharacter")
            .GetValueAsync();
            yield return new WaitUntil(() => characterTask.IsCompleted);
            if (characterTask.Exception != null)
            {
                Index = 1;
                characterName = "Bazzi";
            }
            else
            {
                characterName = characterTask.Result.Value.ToString();
                GetPlayerSelectedCharacter();
            }
        }
        else
        {
            Index = 1;
            characterListPrefab[Index].gameObject.SetActive(true);
        }
    }
    private void GetPlayerSelectedCharacter()
    {
        foreach (var character in characters)
        {
            if (characterName == character.characterName)
            {
                characterListPrefab[Index].gameObject.SetActive(true);
                break;
            }
            Index++;
        }
    }
}
