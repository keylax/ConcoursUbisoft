using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemPool : MonoBehaviour
{
    [SerializeField] private GameObject objectPickerPrefab;
    private readonly List<PickObject.ObjectName> _pooledItems;

    private ItemPool()
    {
        _pooledItems = new List<PickObject.ObjectName>(System.Enum.GetValues(typeof(PickObject.ObjectName)).Cast<PickObject.ObjectName>());
    }
    
    private void Awake()
    {
        StaticObjects.ItemPool = this;
    }
    
    public void SpawnItem(Vector3 position)
    {
        PickObject.ObjectName itemName = _pooledItems[Random.Range(0, _pooledItems.Count)];
        _pooledItems.Remove(itemName);
        
        SpawnItem(position, itemName, true);
    }

    public void SpawnItem(Vector3 position, PickObject.ObjectName itemName, bool canPlayVoiceLine = false)
    {
        SpawnItem(position, itemName, Quaternion.identity, canPlayVoiceLine);
    }
    
    public GameObject SpawnItem(Vector3 position, PickObject.ObjectName itemName, Quaternion rotation, bool canPlayVoiceLine = false)
    {
        GameObject item = Instantiate(objectPickerPrefab, position, rotation);
        item.GetComponentInChildren<PickObject>().Init(itemName, canPlayVoiceLine);
        
        return item;
    }
}
