using System.Collections.Generic;
using UnityEngine;

namespace DonutPackage.ObjectPooling
{
    [CreateAssetMenu(fileName = "ObjectPoolCollection", menuName = "DonutPackage/ObjectPool/ObjectPool Collection", order = 90)]
    public class ObjectPoolCollection : ScriptableObject
    {
        public List<PoolInfo> pools = new List<PoolInfo>();
    }
}