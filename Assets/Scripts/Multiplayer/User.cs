using System;
using UnityEngine;
[Serializable]
public class User 
{
    #region Fields

    static int fields = 3;
    [SerializeField]
    string name;
    [SerializeField]
    int avatar;
    string id;
    
    #endregion

    #region Constractor
    public User(string _name = null, int _avatar = -1, string _id = null)
    {
        // generate a random id to guarantee every user is unique 
        id = _id ?? System.Guid.NewGuid().ToString();
        name = _name;
        avatar = _avatar == -1 ? UnityEngine.Random.Range(1, SC_GameData.Instance.NumberOfChars + 1) : _avatar;
    }
    #endregion

    #region API
    public override string ToString()
    {
        return name +"+"+ avatar +"+"+ id;
    }
    public static User FromString(string _string)
    {
        if (_string == null || !_string.Contains('+')) { 
            Debug.LogError("Failed to get user from string! string is null or invalid."); 
            return null;
        }
        string[] splitString = _string.Split("+");  
        if (splitString.Length != fields || !int.TryParse(splitString[1], out int _avatar)) {
            Debug.LogError($"Failed to get user from string! string is invalid \n({_string}).");
            return null;
        }
        string _name = splitString[0];
        string _id = splitString[2];
        return new User(_name, _avatar, _id);
    }
    public string Name { get => name; set => name = value; }
    public int Avatar { get => avatar; set => avatar = value; }
    #endregion
}
