using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// Finds and returns a game objects component. If not found adds a new one.
    /// <para></para>
    /// <paramref name="obj"/> The game object to add component to.
    /// <para></para>
    /// <typeparamref name="T"/> Component type to add.
    /// </summary>
    /// <returns>The component that was initialized</returns>
    public static T InitComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }
        if (component == null)
        {
            Debug.LogError($"Failed to Initialize {obj.name}! {typeof(T)} is null.");
            return null;
        }
        return component;
    }
}
