using UnityEngine;
namespace TomoClub.Util
{

	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		[SerializeField] protected bool dontDestoryOnLoad = false;
		[Tooltip("Destroy's the gameobject on which this script is attached, if false only destroy's the class")]
		[SerializeField] protected bool destroyGameObjectOnDuplicate = false;

		private static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (T)FindObjectOfType(typeof(T));
				}
				return instance;
			}
		}

		public virtual void Awake()
		{
			if (instance == null)
			{
				instance = this as T;
				if (dontDestoryOnLoad) DontDestroyOnLoad(gameObject);
			}
			else
			{
				if (destroyGameObjectOnDuplicate) Destroy(this.gameObject);
				else Destroy(this);
			}
		}


	} 
}