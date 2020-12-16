﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectComponents.Abstraction;
using SystemFacade;
using UnityEngine;

namespace ApplicationFacade
{
    public class Container
    {
        public delegate void ContainerChangedEventHandler( StorageData storage );

        public event ContainerChangedEventHandler ContainerCreated;

        public event ContainerChangedEventHandler ContainerDeleted;

        public List<StorageData> ContainerData { get; private set; }

        internal InternalProjectContainer Data { get; private set; }

        public Container()
        {
            ContainerData = new List<StorageData>( );
            Data = new InternalProjectContainer( );
        }

        public StorageData CreateContainer( Vector3 position, Quaternion rotation, Vector3 scale )
        {
            LogManager.WriteInfo( "Mobiles Regal wird erstellt.", "ContainerData", "CreateContainer" );

            StorageData container = new StorageData( GetUniqueID( ContainerData.ToArray( ) ), position, rotation, scale );

            container.GameObjectDataChanged += GameObjectHasChanged;
            container.StorageChanged += ContainerHasChanged;

            ContainerData.Add( container );

            Data.Container.Add( new ProjectStorageData( container.GetID( ), container.SlotCount, new ProjectTransformationData( position, rotation, scale ) ) );

            OnSContainerCreated( container );

            return container;
        }

        internal void AddContainer( StorageData container )
        {
            LogManager.WriteInfo( "Mobiles Regal wird hinzugefuegt.", "Warehouse", "AddContainer" );

            container.GameObjectDataChanged += GameObjectHasChanged;
            container.StorageChanged += ContainerHasChanged;

            ContainerData.Add( container );

            Data.Container.Add( new ProjectStorageData( container.GetID( ), container.SlotCount, new ProjectTransformationData( container.Position, container.Rotation, container.Scale ) ) );
        }

        public bool RemoveContainer( StorageData container )
        {
            LogManager.WriteInfo( "Mobiles Regal wird entfernt.", "ContainerData", "RemoveContainer" );

            if ( !ContainerData.Remove( container ) )
            {
                return false;
            }

            for ( int i = 0; i < Data.Container.Count; i++ )
            {
                if ( Data.Container[ i ].ID == container.GetID( ) )
                {
                    container.GameObjectDataChanged -= GameObjectHasChanged;
                    container.StorageChanged -= ContainerHasChanged;

                    Data.Container.Remove( Data.Container[ i ] );

                    OnContainerDeleted( container );

                    return true;
                }
            }

            return false;
        }

        public StorageData GetContainer( long id )
        {
            LogManager.WriteInfo( "Mobiles Regal wird abgefragt.", "ContainerData", "GetContainer" );

            for ( int i = 0; i < ContainerData.Count; i++ )
            {
                if ( ContainerData[ i ].GetID( ) == id )
                {
                    return ContainerData[ i ];
                }
            }

            return null;
        }

        public StorageData GetContainer( GameObject obj )
        {
            LogManager.WriteInfo( "Mobiles Regal wird abgefragt.", "ContainerData", "GetContainer" );

            for ( int i = 0; i < ContainerData.Count; i++ )
            {
                if ( ContainerData[ i ].Object == obj )
                {
                    return ContainerData[ i ];
                }
            }

            return null;
        }

        public ItemData CreateContainerItem( Vector3 position, Quaternion rotation, Vector3 scale, StorageData container, int slot )
        {
            LogManager.WriteInfo( "Ein ContainerItem wird erstellt.", "ContainerData", "CreateContainerItem" );
            

            return null;
        }

        public void AddItemToContainer( StorageData container, ItemData item, int slot )
        {
            LogManager.WriteInfo( "Ein ContainerItem wird erstellt.", "ContainerData", "AddItemToContainer" );
                     
            
        }

        public bool RemoveItemFromContainer( StorageData storage, ItemData item )
        {
            LogManager.WriteInfo( "Ein ContainerItem wird entfernt.", "ContainerData", "RemoveItemFromContainer" );
            

            return false;
        }

        private long GetUniqueID( IDataIdentifier[ ] idUsed )
        {
            bool used = false;

            for ( int i = 0; ; i++ )
            {
                used = false;

                for ( int j = 0; j < idUsed.Length; j++ )
                {
                    if ( i + 1 == idUsed[ j ].GetID( ) )
                    {
                        used = true;
                        break;
                    }
                }

                if ( !used )
                {
                    return i + 1;
                }
            }
        }
        
        private void GameObjectHasChanged( GameObjectData obj, GameObjectDataType type )
        {
            LogManager.WriteInfo( "[Event]Aktualisiere GameObjectData. Type=" + type.ToString( ), "Container", "GameObjectHasChanged" );

            switch ( type )
            {
                case GameObjectDataType.StorageReck:

                    ContainerHasChanged( obj as StorageData );
                    break;

                case GameObjectDataType.Item:

                    ContainerItemHasChanged( obj as ItemData );
                    break;

                default:

                    LogManager.WriteWarning( "[Event] Falscher Typ in EventSystem referenziert!", "Container", "GameObjectHasChanged" );
                    break;
            }
        }

        private void ContainerHasChanged( StorageData storage )
        {
            LogManager.WriteInfo( "[Event]Aktualisiere StorageData.", "Warehouse", "StorageRackHasChanged" );

            for ( int i = 0; i < Data.Container.Count; i++ )
            {
                if ( storage.GetID( ) == Data.Container[ i ].ID )
                {
                    if ( storage.GetItems.Length > Data.Container[ i ].GetItems.Length )
                    {
                        ProjectTransformationData data = new ProjectTransformationData( storage.GetItems[ storage.GetItems.Length - 1 ].Position,
                                                                                        storage.GetItems[ storage.GetItems.Length - 1 ].Rotation,
                                                                                        storage.GetItems[ storage.GetItems.Length - 1 ].Scale );

                        Data.Container[i].Items.Add( new ProjectItemData( storage.GetItems[storage.GetItems.Length - 1].GetID( ),
                                                       storage.GetItems[storage.GetItems.Length - 1].Count,
                                                       storage.GetItems[storage.GetItems.Length - 1].Weight,
                                                       storage.GetItems[storage.GetItems.Length - 1].Name,
                                                       data ) );

                        break;
                    }

                    else if ( storage.GetItems.Length < Data.Container[ i ].GetItems.Length )
                    {
                        if ( storage.GetItems.Length == 1 )
                        {
                            Data.Container[ i ].Items.Remove( Data.Container[ i ].GetItems[ 0 ] );

                            break;
                        }

                        for ( int j = 0; j < Data.Container[ i ].GetItems.Length; j++ )
                        {
                            if ( Data.Container[ i ].GetItems[ j ].IDRef != storage.GetItems[ j ].GetID( ) )
                            {
                                Data.Container[ i ].Items.Remove( Data.Container[ i ].GetItems[ j ] );

                                break;
                            }
                        }

                        break;
                    }

                    else
                    {
                        ProjectItemData[ ] items = Data.Container[ i ].GetItems;
                        ProjectStorageData data = new ProjectStorageData( storage.GetID( ), Data.Container[i].SlotCount, new ProjectTransformationData( storage.Position, storage.Rotation, storage.Scale ) );

                        foreach ( ProjectItemData item in items )
                        {
                            data.Items.Add( item );
                        }

                        Data.Container.Remove( Data.Container[ i ] );
                        Data.Container.Insert( i, data );

                        break;
                    }
                }
            }
        }

        private void ContainerItemHasChanged( ItemData item )
        {
            LogManager.WriteInfo( "[Event]Aktualisiere ItemData.", "Warehouse", "StorageRackItemHasChanged" );

           // for ( int i = 0; i < ContainerData.Count; i++ )
           // {
           //     if ( item.Parent.GetID( ) == ContainerData[ i ].GetID( ) )
           //     {
           //         for ( int j = 0; j < Data.Container[ i ].GetItems.Length; j++ )
           //         {
           //             if ( Data.Container[ i ].GetItems[ j ].IDRef == item.GetID( ) )
           //             {
           //                 ProjectTransformationData data = new ProjectTransformationData( item.Position, item.Rotation, item.Scale );
           //
           //                 Data.Container[ i ].Items.Remove( Data.Container[ i ].GetItems[ j ] );
           //                 Data.Container[ i ].Items.Insert( j, new ProjectItemData( item.GetID( ), item.Count, item.Weight, item.Name, new ProjectTransformationData( item.Position, item.Rotation, item.Scale ) ) );
           //
           //                 break;
           //             }
           //         }
           //
           //         break;
           //     }
           // }
        }  //
        
        protected virtual void OnSContainerCreated( StorageData data )
        {
            ContainerCreated?.Invoke( data );
        }

        protected virtual void OnContainerDeleted( StorageData data )
        {
            ContainerDeleted?.Invoke( data );
        }
    }
}