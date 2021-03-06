﻿using System.Collections;
using System.Collections.Generic;
using ProjectComponents.FileIntegration;
using ProjectComponents.Abstraction;
using SystemFacade;
using System;
using UnityEngine;
using System.IO;
using AppData.Warehouse;

namespace ApplicationFacade
{
    public class ProjectManager
    {
        public ProjectData Data { get; private set; }
        public ProjectSettings Settings { get; private set; }
        
        internal DataHandler      DHandler { get; set; }
        internal SettingsHandler  SHandler { get; set; }
        internal WarehouseHandler WHandler { get; set; }
        internal ContainerHandler CHandler { get; set; }
        internal StockHandler     IHandler { get; set; }

        public ProjectData[] ProjectList
        {
            get
            {
                ProjectData[] data = new ProjectData[Directory.GetFiles(Paths.ProjectsPath).Length];

                for ( int i = 0; i < data.Length; i++ )
                {
                    ArchiveManager.ExtractArchive( Directory.GetFiles(Paths.ProjectsPath)[i], Paths.TempPath );

                    DataHandler dhandler = new DataHandler( );
                    data[i] = new ProjectData( );

                    ReadProjectData( data[i], dhandler.LoadFile() );

                    Paths.ClearTempPath( );
                }

                return data;
            }
        }

        public string ProjectName
        {
            get
            {
                if ( Data == null )
                {
                    return null;
                }

                return Data.ProjectName;
            }

            private set
            {
                Data.ProjectName = value;
            }
        }

        public ProjectManager()
        {
        }

        ~ProjectManager()
        {
            if ( !ProjectName.Equals( string.Empty ) )
            {
                CloseProject( );
            }
        }

        public void LoadProject( string name, ref Warehouse warehouse, ref Container container )
        {
            if ( !File.Exists( Paths.ProjectsPath + name + ".prj" ) )
            {
                warehouse = null;
                container = null;

                return;
            }

            WallData.NorthWallLength = 0;
            WallData.EastWallLength  = 0;
            WallData.SouthWallLength = 0;
            WallData.WestWallLength  = 0;

            ArchiveManager.ExtractArchive( Paths.ProjectsPath + name + ".prj", Paths.TempPath );
            
            DHandler = new DataHandler( );
            SHandler = new SettingsHandler( );
            WHandler = new WarehouseHandler( );
            CHandler = new ContainerHandler( );
            IHandler = new StockHandler( );
            
            InternalProjectData idata = DHandler.LoadFile( );
            Data = new ProjectData( );
            ReadProjectData( Data, idata );

            InternalProjectSettings isettings = SHandler.LoadFile( );
            Settings = new ProjectSettings( );
            ReadProjectSettings( isettings );
            
            ReadProjectStock( );

            InternalProjectWarehouse iwarehouse = WHandler.LoadFile( );
            ReadProjectWarehouse( iwarehouse, warehouse );
            
            InternalProjectContainer icontainer = CHandler.LoadFile( );
            ReadProjectContainer( icontainer, container );
        }

        public void SaveProject( string name, Warehouse warehouse, Container container )
        {
            if ( name != null )
            {
                if ( !name.Equals( string.Empty ) )
                {
                    ProjectName = name;
                }
            }

            WriteProjectData( );
            WriteProjectSettings( );
            WriteProjectWarehouse( warehouse );
            WriteProjectContainer( container );
            WriteProjectStock( );

            if ( File.Exists( Data.ProjectPath + Data.ProjectName + InternalProjectData.Extension ) )
            {
                File.Delete( Data.ProjectPath + Data.ProjectName + InternalProjectData.Extension );
            }

            ArchiveManager.ArchiveDirectory( Paths.TempPath, Data.ProjectPath + ProjectName + InternalProjectData.Extension );
        }

        public void CloseProject()
        {
            Data = null;
            Settings = null;

            DHandler = new DataHandler( );
            WHandler = new WarehouseHandler( );
            SHandler = new SettingsHandler( );
            CHandler = new ContainerHandler( );
            IHandler = new StockHandler( );

            Paths.ClearTempPath( );
        }

        public void CreateProject( string name, WarehouseSize size, ref Warehouse warehouse, ref Container container )
        {
            if ( ProjectName != null )
            {
                CloseProject( );
            }

            if ( File.Exists( Paths.ProjectsPath + name + InternalProjectData.Extension ) )
            {
                warehouse = null;
                container = null;

                return;
            }

            WallData.NorthWallLength = 0;
            WallData.EastWallLength = 0;
            WallData.SouthWallLength = 0;
            WallData.WestWallLength = 0;

            DHandler = new DataHandler( );
            SHandler = new SettingsHandler( );
            WHandler = new WarehouseHandler( );
            CHandler = new ContainerHandler( );
            IHandler = new StockHandler( );

            Data = new ProjectData( );
            ProjectName = name;
            Data.ProjectPath = Paths.ProjectsPath;

            Settings  = new ProjectSettings( );

            container = GetDefaultContainer( );

            switch ( size )
            {
                case WarehouseSize.Small:
                    
                    warehouse = GetDefaultWarehouse( new SmallWarehouseStrategie() );
                    break;

                case WarehouseSize.Medium:

                    warehouse = GetDefaultWarehouse( new MediumWarehouseStrategie() );
                    break;

                case WarehouseSize.Large:

                    warehouse = GetDefaultWarehouse( new LargeWarehouseStrategie() );
                    break;
            }

            SaveProject( name, warehouse, GetDefaultContainer( ) );
        }

        public void DeleteProject( string name )
        {
            if( ProjectName != null && ProjectName.Equals( name ) )
            {
                CloseProject( );
            }

            if ( File.Exists( Paths.ProjectsPath + name + InternalProjectData.Extension ) )
            {
                File.Delete( Paths.ProjectsPath + name + InternalProjectData.Extension );
            }
        }

        private void WriteProjectStock()
        {
            ProjectItemData[] data = new ProjectItemData[ItemData.GetStock.Length];

            for( int i = 0; i < data.Length; i++ )
            {
                data[i] = new ProjectItemData( ItemData.GetStock[i].GetID(), ItemData.GetStock[i].StockCount, ItemData.GetStock[i].Weight, ItemData.GetStock[i].Name, new ProjectTransformationData() );
            }

            IHandler.SaveFile( data );
        }

        private void WriteProjectData( )
        {
            DHandler.SaveFile( Data.Data );
        }

        private void WriteProjectSettings( )
        {
            SHandler.SaveFile( Settings.Data );
        }

        private void WriteProjectWarehouse( Warehouse warehouse )
        {
            WHandler.SaveFile( warehouse.Data );
        }

        private void WriteProjectContainer( Container container )
        {
            CHandler.SaveFile( container.Data );
        }

        private void ReadProjectStock()
        {
            foreach( ProjectItemData item in IHandler.LoadFile() )
            {
                ItemData.AddItemToStock( item.Name, item.Count, item.Weight );
            }
        }

        private void ReadProjectData( ProjectData pData, InternalProjectData data )
        {
            pData.DateCreated  = data.DateCreated;
            pData.DateModified = DateTime.Today;
            pData.ProjectName  = data.Name;
            pData.ProjectPath  = data.FullPath;
        }

        private void ReadProjectSettings( InternalProjectSettings settings )
        {

        }

        private void ReadProjectWarehouse( InternalProjectWarehouse iwarehouse, Warehouse warehouse )
        {
            for( int i = 0; i < iwarehouse.Floor.Count; i++ )
            {
                FloorData floor = new FloorData( iwarehouse.Floor[i].ID )
                {
                    Position = iwarehouse.Floor[i].Transformation.Position,
                    Rotation = iwarehouse.Floor[i].Transformation.Rotation,
                    Scale    = iwarehouse.Floor[i].Transformation.Scale
                };

                warehouse.AddFloor( floor );
            }

            for( int i = 0; i < iwarehouse.Walls.Count; i++ )
            {
                WallData wall = new WallData( iwarehouse.Walls[i].ID )
                {
                    Position = iwarehouse.Walls[i].Transformation.Position,
                    Rotation = iwarehouse.Walls[i].Transformation.Rotation,
                    Scale = iwarehouse.Walls[i].Transformation.Scale,
                    Face  =  (WallFace)Enum.Parse( typeof( WallFace ), iwarehouse.Walls[i].Face ),
                    Class = (WallClass)Enum.Parse( typeof( WallClass ), iwarehouse.Walls[i].Class )
                };
                
                warehouse.AddWall( wall );
            }
            
            warehouse.Walls[0].Object.tag = "LeftWallRim";
            warehouse.Walls[WallData.NorthWallLength - 1].Object.tag = "RightWallRim";
            warehouse.Walls[WallData.NorthWallLength].Object.tag = "LeftWallRim";
            warehouse.Walls[WallData.NorthWallLength + WallData.EastWallLength - 1].Object.tag = "RightWallRim";
            warehouse.Walls[WallData.NorthWallLength + WallData.EastWallLength].Object.tag = "LeftWallRim";
            warehouse.Walls[WallData.NorthWallLength + WallData.EastWallLength + WallData.SouthWallLength - 1].Object.tag = "RightWallRim";
            warehouse.Walls[WallData.NorthWallLength + WallData.EastWallLength + WallData.SouthWallLength].Object.tag = "LeftWallRim";
            warehouse.Walls[WallData.NorthWallLength + WallData.EastWallLength + WallData.SouthWallLength + WallData.WestWallLength - 1].Object.tag = "RightWallRim";

            for ( int i = 0; i < iwarehouse.Windows.Count; i++ )
            {
                WindowData window = new WindowData( iwarehouse.Windows[ i ].ID )
                {
                    Position = iwarehouse.Windows[i].Transformation.Position,
                    Rotation = iwarehouse.Windows[i].Transformation.Rotation,
                    Scale = iwarehouse.Windows[i].Transformation.Scale
                };

                warehouse.AddWindow( window );
            }

            for ( int i = 0; i < iwarehouse.Doors.Count; i++ )
            {
                DoorData door = new DoorData( iwarehouse.Doors[i].ID )
                {
                    Position = iwarehouse.Doors[i].Transformation.Position,
                    Rotation = iwarehouse.Doors[i].Transformation.Rotation,
                    Scale    = iwarehouse.Doors[i].Transformation.Scale,
                    Type     = (DoorType) Enum.Parse( typeof( DoorType ), iwarehouse.Doors[i].Type )
                };

                warehouse.AddDoor( door );
            }

            for ( int i = 0; i < iwarehouse.StorageRacks.Count; i++ )
            {
                StorageData data = new StorageData( iwarehouse.StorageRacks[ i ].ID )
                {
                    Position = iwarehouse.StorageRacks[i].Transformation.Position,
                    Rotation = iwarehouse.StorageRacks[i].Transformation.Rotation,
                    Scale = iwarehouse.StorageRacks[i].Transformation.Scale,
                    SlotCount = iwarehouse.StorageRacks[i].SlotCount
                };

                warehouse.AddStorageRack( data );

                for ( int j = 0; j < iwarehouse.StorageRacks[i].GetItems.Length; j++ )
                {
                    ItemData item = ItemData.RequestStockItem( iwarehouse.StorageRacks[i].GetItems[j].Name );

                    warehouse.AddItemToStorageRack( data, item.RequestItem( iwarehouse.StorageRacks[i].GetItems[j].Count ), j );
                }
            }
        }

        private void ReadProjectContainer( InternalProjectContainer icontainer, Container container )
        {
            for( int i = 0; i < icontainer.Container.Count; i++ )
            {
                StorageData data = new StorageData( icontainer.Container[i].ID )
                {
                    Position = icontainer.Container[i].Transformation.Position,
                    Rotation = icontainer.Container[i].Transformation.Rotation,
                    Scale = icontainer.Container[i].Transformation.Scale,
                    SlotCount = icontainer.Container[i].SlotCount
                };

                container.AddContainer( data );

                for ( int j = 0; j < icontainer.Container[ i ].GetItems.Length; j++ )
                {
                    ItemData item = ItemData.RequestStockItem( icontainer.Container[i].GetItems[j].Name );

                    container.AddItemToContainer( data, item.RequestItem( icontainer.Container[i].GetItems[j].Count ), j );
                }
            }
        }

        private Warehouse GetDefaultWarehouse( IWarehouseStrategie size )
        {
            Warehouse warehouse = new Warehouse( );

            size.StartGeneration( );

            //Floor
            foreach( ObjectTransformation data in size.GetFloor() )
            {
                warehouse.CreateFloor( data.Position, data.Rotation, data.Scale );
            }

            //Walls

            ObjectTransformation[] n = size.GetNorthWalls();
            ObjectTransformation[] e = size.GetEastWalls();
            ObjectTransformation[] s = size.GetSouthWalls();
            ObjectTransformation[] w = size.GetWestWalls();

            foreach ( ObjectTransformation data in n )
            {
                warehouse.CreateWall( data.Position, data.Rotation, data.Scale, WallFace.North, WallClass.Outer );
            }

            warehouse.Walls[0].Object.tag = "LeftWallRim";
            warehouse.Walls[n.Length - 1].Object.tag = "RightWallRim";

            foreach ( ObjectTransformation data in e )
            {
                warehouse.CreateWall( data.Position, data.Rotation, data.Scale, WallFace.East, WallClass.Outer );
            }

            warehouse.Walls[n.Length].Object.tag = "LeftWallRim";
            warehouse.Walls[e.Length + n.Length - 1].Object.tag = "RightWallRim";

            foreach ( ObjectTransformation data in s )
            {
                warehouse.CreateWall( data.Position, data.Rotation, data.Scale, WallFace.South, WallClass.Outer );
            }

            warehouse.Walls[e.Length + n.Length].Object.tag = "LeftWallRim";
            warehouse.Walls[s.Length + e.Length + n.Length - 1].Object.tag = "RightWallRim";

            foreach ( ObjectTransformation data in w )
            {
                warehouse.CreateWall( data.Position, data.Rotation, data.Scale, WallFace.West, WallClass.Outer );
            }

            warehouse.Walls[s.Length + e.Length + n.Length].Object.tag = "LeftWallRim";
            warehouse.Walls[w.Length + s.Length + e.Length + n.Length - 1].Object.tag = "RightWallRim";

            //Windows
            foreach ( ObjectTransformation data in size.GetWindows( ) )
            {
                warehouse.CreateWindow( data.Position, data.Rotation, data.Scale );
            }

            //Doors
            foreach ( ObjectTransformation data in size.GetDoors( ) )
            {
                warehouse.CreateDoor( data.Position, data.Rotation, data.Scale, DoorType.Door );
            }

            //StorageRackList
            foreach ( ObjectTransformation data in size.GetStorageRacks( ) )
            {
                warehouse.CreateStorageRack( data.Position, data.Rotation, data.Scale );
            }

            return warehouse;
        }

        private Container GetDefaultContainer()
        {
            Container container = new Container( );

            return container;
        }
    }
}