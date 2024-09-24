/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using System.Diagnostics;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.Serialization;

// Allows for fast generation of valid (inside the room, outside furniture bounds) random positions for content spawning.
// Optional method to pin directly to surfaces
public class FindLargestSpawnPositions : MonoBehaviour
{
    [Tooltip("When the scene data is loaded, this controls what room(s) the prefabs will spawn in.")]
    public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

    [SerializeField, Tooltip("Prefab to be placed into the scene, or object in the scene to be moved around.")]
    public GameObject SpawnObject;

    [SerializeField, Tooltip("Maximum number of times to attempt spawning/moving an object before giving up.")]
    public int MaxIterations = 1000;

    public enum SpawnLocation
    {
        Floating, // Spawn somewhere floating in the free space within the room
        AnySurface, // Spawn on any surface (i.e. a combination of all 3 options below)
        VerticalSurfaces, // Spawn only on vertical surfaces such as walls, windows, wall art, doors, etc...
        OnTopOfSurfaces, // Spawn on surfaces facing upwards such as ground, top of tables, beds, couches, etc...
        HangingDown // Spawn on surfaces facing downwards such as the ceiling
    }

    [FormerlySerializedAs("selectedSnapOption")]
    [SerializeField, Tooltip("Attach content to scene surfaces.")]
    public SpawnLocation SpawnLocations = SpawnLocation.Floating;

    [SerializeField, Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
    public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;

    [SerializeField, Tooltip("If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.")]
    public bool CheckOverlaps = true;

    [FormerlySerializedAs("layerMask")]
    [SerializeField, Tooltip("Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.")]
    public LayerMask LayerMask = -1;

    [SerializeField, Tooltip("The clearance distance required in front of the surface in order for it to be considered a valid spawn position")]
    public float SurfaceClearanceDistance = 0.1f;

    [SerializeField, Tooltip("Maximum surface size")]
    public float MaximumSpawnRadius = 2f;

    [SerializeField, Tooltip("Minimum surface size")]
    public float MinimumSpawnRadius = 0.5f;

    public bool HideMarker = false;

    private bool _spawnPositionFound = false;

    private void Start()
    {
        // #if UNITY_EDITOR
        //         OVRTelemetry.Start(TelemetryConstants.MarkerId.LoadFindSpawnPositions).Send();
        // #endif
        if (MRUK.Instance && SpawnOnStart != MRUK.RoomFilter.None)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                switch (SpawnOnStart)
                {
                    case MRUK.RoomFilter.AllRooms:
                        StartSpawn();
                        break;
                    case MRUK.RoomFilter.CurrentRoomOnly:
                        StartSpawn(MRUK.Instance.GetCurrentRoom());
                        break;
                }
            });
        }

        if (HideMarker)
        {
            var renderers = SpawnObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
    }

    public void StartSpawn()
    {
        foreach (var room in MRUK.Instance.Rooms)
        {
            StartSpawn(room);
        }
    }

    public void StartSpawnCurrentRoom()
    {
        var room = MRUK.Instance.GetCurrentRoom();
        if (room != null)
        {
            StartSpawn(room);
        }
        else
        {
            UnityEngine.Debug.Log("FindLargestSpawnPositions: Cannot find current room");
        }
    }

    public void StartSpawn(MRUKRoom room)
    {
        _spawnPositionFound = false;

        for (float i = MaximumSpawnRadius; i >= MinimumSpawnRadius; i -= 0.1f)
        {
            UnityEngine.Debug.Log("Finding spawn position at radius: " + i + " meters");
            for (int j = 0; j < MaxIterations; ++j)
            {
                Vector3 spawnPosition = Vector3.zero;
                Vector3 spawnNormal = Vector3.zero;
                if (SpawnLocations == SpawnLocation.Floating)
                {
                    var randomPos = room.GenerateRandomPositionInRoom(i, true);
                    if (!randomPos.HasValue)
                    {
                        break;
                    }

                    spawnPosition = randomPos.Value;
                }
                else
                {
                    MRUK.SurfaceType surfaceType = 0;
                    switch (SpawnLocations)
                    {
                        case SpawnLocation.AnySurface:
                            surfaceType |= MRUK.SurfaceType.FACING_UP;
                            surfaceType |= MRUK.SurfaceType.VERTICAL;
                            surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                            break;
                        case SpawnLocation.VerticalSurfaces:
                            surfaceType |= MRUK.SurfaceType.VERTICAL;
                            break;
                        case SpawnLocation.OnTopOfSurfaces:
                            surfaceType |= MRUK.SurfaceType.FACING_UP;
                            break;
                        case SpawnLocation.HangingDown:
                            surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                            break;
                    }

                    if (room.GenerateRandomPositionOnSurface(surfaceType, i, LabelFilter.Included(Labels), out var pos, out var normal))
                    {
                        spawnPosition = pos;
                        spawnNormal = normal;
                        var center = spawnPosition;
                        // In some cases, surfaces may protrude through walls and end up outside the room
                        // check to make sure the center of the prefab will spawn inside the room
                        if (!room.IsPositionInRoom(center))
                        {
                            continue;
                        }

                        // Ensure the center of the prefab will not spawn inside a scene volume
                        if (room.IsPositionInSceneVolume(center))
                        {
                            continue;
                        }

                        // Also make sure there is nothing close to the surface that would obstruct it
                        if (room.Raycast(new Ray(pos, normal), SurfaceClearanceDistance, out _))
                        {
                            continue;
                        }
                    }
                }

                Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
                if (CheckOverlaps)
                {
                    if (Physics.CheckBox(spawnPosition, new Vector3(i, 0.05f, i), spawnRotation, LayerMask, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }
                }

                SpawnObject.transform.localScale = new Vector3(i * 2, 0.05f, i * 2);
                Instantiate(SpawnObject, spawnPosition, spawnRotation, transform);
                _spawnPositionFound = true;
                UnityEngine.Debug.Log("Spawned object at radius: " + i + " meters");

                break;
            }

            if (_spawnPositionFound)
            {
                break;
            }
        }

        // If nothing is found, spawn in the middle of the room, or where the user currently stands
        if (!_spawnPositionFound)
        {
            UnityEngine.Debug.Log("Cannot find valid spawn position; spawning at where the user is standing");
            var spawnPosition = Camera.main.transform.position;
            var floorAnchor = room.FloorAnchor;

            if (floorAnchor != null)
            {
                spawnPosition.y = floorAnchor.transform.position.y;
            }
            else
            {
                spawnPosition.y = 0;
            }

            Instantiate(SpawnObject, spawnPosition, Quaternion.identity, transform);
        }
    }
}
