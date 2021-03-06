﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectComponents.Abstraction
{
    public struct ProjectStorageData
    {
        public long ID { get; set; }
        public int SlotCount { get; set; }
        public ProjectTransformationData Transformation { get; set; }
        public List<ProjectItemData> Items { get; set; }

        public ProjectItemData[] GetItems
        {
            get
            {
                return Items.ToArray( );
            }
        }

        public ProjectStorageData( long id, int slotCount, ProjectTransformationData transformation )
        {
            ID = id;
            SlotCount = slotCount;
            Transformation = transformation;
            Items = new List<ProjectItemData>( );
        }        
    }
}
