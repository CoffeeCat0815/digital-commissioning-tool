﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectComponents.Abstraction
{
    public struct ProjectWallData
    {
        public long ID { get; set; }
        public string Face { get; set; }
        public string Class { get; set; }
        public ProjectTransformationData Transformation { get; set; }

        public ProjectWallData( long id, string face, string wClass, ProjectTransformationData transformation )
        {
            ID = id;
            Face = face;
            Class = wClass;
            Transformation = transformation;
        }        
    }
}
