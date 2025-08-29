import React, { useEffect, useRef, useState } from 'react';
import 'ol/ol.css';
import Map from 'ol/Map';
import View from 'ol/View';
import { Tile as TileLayer, Vector as VectorLayer } from 'ol/layer';
import { OSM, Vector as VectorSource } from 'ol/source';
import { fromLonLat } from 'ol/proj';
import { Feature } from 'ol';
import GeoJSON from 'ol/format/GeoJSON';
import { Geometry, Polygon as OLPolygon, SimpleGeometry } from 'ol/geom';
import { Draw, Select } from 'ol/interaction';
import { getCenter } from 'ol/extent';

import { GeometryFactory } from 'jsts/org/locationtech/jts/geom';
import { GeoJSONReader, GeoJSONWriter } from 'jsts/org/locationtech/jts/io';
import { OverlayOp } from 'jsts/org/locationtech/jts/operation/overlay';

import { type Shape, type Geometry as ShapeGeometry } from '../services/shapeService';
import Popup from './Popup';
import InfoPopup from './InfoPopup';
import { styleFunction } from './style';

interface MapComponentProps {
    shapes: Shape[];
    drawType: 'Point' | 'LineString' | 'Polygon' | 'None';
    onDrawEnd: (geometry: Geometry) => void;
    focusGeometry?: ShapeGeometry | null;
    resetViewToggle: boolean;
    isMergeMode: boolean;
    onMerge: (name: string, geometry: ShapeGeometry, deleteIds: number[]) => void;
}

const MapComponent: React.FC<MapComponentProps> = ({ shapes, drawType, onDrawEnd, focusGeometry, resetViewToggle, isMergeMode, onMerge }) => {
    const mapElement = useRef<HTMLDivElement>(null);
    const mapRef = useRef<Map | null>(null);
    const drawInteractionRef = useRef<Draw | null>(null);
    const selectInteractionRef = useRef<Select | null>(null);
    const vectorSourceRef = useRef<VectorSource>(new VectorSource());
    const [popupContent, setPopupContent] = useState('');
    const [popupPosition, setPopupPosition] = useState({ x: 0, y: 0 });
    const [selectedShape, setSelectedShape] = useState<Shape | null>(null);
    const [containedShapes, setContainedShapes] = useState<Shape[]>([]);
    const [infoPopupPosition, setInfoPopupPosition] = useState<{ x: number, y: number } | null>(null);
    const [selectedFeaturesForMerge, setSelectedFeaturesForMerge] = useState<Feature[]>([]);

    const handleMerge = () => {
        console.log("Merge button clicked");
        if (selectedFeaturesForMerge.length !== 2) {
            console.error("Merge handle error: 2 polygons must be selected.");
            return;
        }

        const idsToDelete = selectedFeaturesForMerge.map(f => {
            const featureName = f.get('name');
            const shape = shapes.find(s => s.name === featureName);
            return shape ? shape.id : -1;
        }).filter(id => id !== -1);
        
        const newName = selectedFeaturesForMerge.map(f => f.get('name')).join(' + ');

        const geojsonFormat = new GeoJSON({
            featureProjection: 'EPSG:3857',
            dataProjection: 'EPSG:4326'
        });
        
        try {
            const geometryFactory = new GeometryFactory();
            const reader = new GeoJSONReader(geometryFactory);
            const geometries = selectedFeaturesForMerge.map(feature => {
                const geojson = geojsonFormat.writeFeatureObject(feature);
                return reader.read(geojson.geometry);
            });
    
            const mergedGeometry = OverlayOp.union(geometries[0], geometries[1]); // seçili 2 geometri objesini alıp  union işlemi yapıyor.
            
            console.log("Merged JSTS Geometry:", mergedGeometry);
    
            if (!mergedGeometry || mergedGeometry.isEmpty()) {
                console.error("Merge result is empty or invalid.");
                alert("Could not merge selected polygons. They might not be adjacent or may be invalid.");
                return;
            }
            
            const writer = new GeoJSONWriter();
            const mergedGeoJSON = writer.write(mergedGeometry);
    
            console.log("Calling onMerge with:", { newName, mergedGeoJSON, idsToDelete });
            onMerge(newName, mergedGeoJSON as ShapeGeometry, idsToDelete);
            setSelectedFeaturesForMerge([]);
        } catch (error) {
            console.error("Error during merge operation:", error);
            alert("An error occurred while merging the polygons. Please check the console for details.");
        }
    };
    
    useEffect(() => {
        if (mapElement.current && !mapRef.current) {
            const vectorSource = vectorSourceRef.current;

            const vectorLayer = new VectorLayer({
                source: vectorSource,
                style: styleFunction
            });

            const map = new Map({
                target: mapElement.current,
                layers: [
                    new TileLayer({
                        source: new OSM(),
                    }),
                    vectorLayer
                ],
                view: new View({
                    center: fromLonLat([35.2433, 38.9637]), // Center of Turkey
                    zoom: 6,
                }),
            });
            mapRef.current = map;
        }

        return () => {
            if (mapRef.current) {
                mapRef.current.setTarget(undefined);
                mapRef.current = null;
            }
        };
    }, []);

    useEffect(() => {
        if (!mapRef.current) return;

        if (isMergeMode) {
            if (drawInteractionRef.current) {
                mapRef.current.removeInteraction(drawInteractionRef.current);
            }
            if (selectInteractionRef.current) {
                mapRef.current.removeInteraction(selectInteractionRef.current);
            }

            const select = new Select({
                multi: true,
                layers: (layer) => layer.getSource() === vectorSourceRef.current,
                filter: (feature) => {
                    const geometry = feature.getGeometry();
                    return geometry ? geometry.getType() === 'Polygon' : false;
                }
            });

            select.on('select', (e) => {
                setSelectedFeaturesForMerge(e.selected);
            });
            
            mapRef.current.addInteraction(select);
            selectInteractionRef.current = select;

        } else {
            if (selectInteractionRef.current) {
                mapRef.current.removeInteraction(selectInteractionRef.current);
                selectInteractionRef.current = null;
                setSelectedFeaturesForMerge([]);
            }
        }
    }, [isMergeMode]);

    useEffect(() => {
        if (!mapRef.current) return;
        const map = mapRef.current;

        const handleClick = (event: any) => {
            if (drawType !== 'None' || isMergeMode) return;

            // Always hide the popup on any click to start fresh.
            setSelectedShape(null);
            setInfoPopupPosition(null);
            setContainedShapes([]);

            const feature = map.forEachFeatureAtPixel(event.pixel, (f) => f);

            if (feature) {
                const shape = shapes.find(s => s.name === feature.get('name'));
                if (shape) {
                    const geom = feature.getGeometry();
                    if (geom) {
                        if (geom.getType() === 'Polygon') {
                            const polygonGeom = geom as OLPolygon;
                            const contained: Shape[] = [];
                            vectorSourceRef.current.forEachFeature((otherFeature) => {
                                if (feature === otherFeature) return;

                                const otherGeom = otherFeature.getGeometry();
                                if (!otherGeom) return;
                                
                                const otherShape = shapes.find(s => s.name === otherFeature.get('name'));
                                if (!otherShape) return;
                                
                                let isContained = false;
                                switch (otherGeom.getType()) {
                                    case 'Point': {
                                        const coordinates = (otherGeom as SimpleGeometry).getCoordinates();
                                        if (coordinates && polygonGeom.intersectsCoordinate(coordinates as number[])) {
                                            isContained = true;
                                        }
                                        break;
                                    }
                                    case 'LineString': {
                                        const coordinates = (otherGeom as SimpleGeometry).getCoordinates();
                                        isContained = !!coordinates && (coordinates as number[][]).every(coord => polygonGeom.intersectsCoordinate(coord));
                                        break;
                                    }
                                    case 'Polygon': {
                                        const polygonCoordinates = (otherGeom as OLPolygon).getCoordinates();
                                        if (polygonCoordinates?.[0]) {
                                            isContained = polygonCoordinates[0].every(coord => polygonGeom.intersectsCoordinate(coord));
                                        }
                                        break;
                                    }
                                }

                                if (isContained) {
                                    contained.push(otherShape);
                                }
                            });
                            setContainedShapes(contained);
                        }

                        map.getView().fit(geom.getExtent(), {
                            padding: [150, 150, 150, 150],
                            duration: 1000,
                            maxZoom: 15,
                            callback: () => {
                                const center = getCenter(geom.getExtent());
                                const pixel = map.getPixelFromCoordinate(center);
                                setSelectedShape(shape);
                                if (pixel) {
                                    setInfoPopupPosition({ x: pixel[0], y: pixel[1] });
                                }
                            }
                        });
                    }
                }
            }
        };

        map.on('click', handleClick);

        return () => {
            map.un('click', handleClick);
        };
    }, [drawType, shapes, isMergeMode]);

    useEffect(() => {
        if (!mapRef.current) return;
        const map = mapRef.current;

        const handlePointerMove = (event: any) => {
            if (event.dragging) {
                setPopupContent('');
                return;
            }
            const pixel = map.getEventPixel(event.originalEvent);
            const feature = map.forEachFeatureAtPixel(pixel, (f) => f);

            if (feature) {
                const featureName = feature.get('name') || 'No name';
                setPopupContent(featureName);

                const geom = feature.getGeometry();
                if (geom) {
                    const center = getCenter(geom.getExtent());
                    const centerPixel = map.getPixelFromCoordinate(center);
                    if (centerPixel) {
                        setPopupPosition({ x: centerPixel[0], y: centerPixel[1] });
                    }
                }
            } else {
                setPopupContent('');
            }
        };

        map.on('pointermove', handlePointerMove);

        return () => {
            map.un('pointermove', handlePointerMove);
        };
    }, []);

    const handleCloseInfoPopup = () => {
        setSelectedShape(null);
        setInfoPopupPosition(null);
    };

    useEffect(() => {
        if (resetViewToggle && mapRef.current) {
            mapRef.current.getView().animate({
                center: fromLonLat([35.2433, 38.9637]),
                zoom: 6,
                duration: 1000,
            });
        }
    }, [resetViewToggle]);

    useEffect(() => {
        if (vectorSourceRef.current) {
            vectorSourceRef.current.clear();
            if (shapes.length > 0) {
                const features = new GeoJSON().readFeatures({
                    type: 'FeatureCollection',
                    features: shapes.map(shape => ({
                        type: 'Feature',
                        geometry: shape.geometry,
                        properties: {
                            name: shape.name,
                            type: shape.type,
                        },
                    })),
                }, {
                    featureProjection: 'EPSG:3857',
                });
                vectorSourceRef.current.addFeatures(features);
            }
        }
    }, [shapes]);

    useEffect(() => {
        if (focusGeometry && mapRef.current) {
            const feature = new GeoJSON().readFeature({
                type: 'Feature',
                geometry: focusGeometry,
                properties: {}
            }, {
                dataProjection: 'EPSG:4326',
                featureProjection: 'EPSG:3857',
            });
            const geom = Array.isArray(feature) ? feature[0]?.getGeometry() : feature.getGeometry();
            if (geom) {
                mapRef.current.getView().fit(geom.getExtent(), {
                    padding: [100, 100, 100, 100],
                    duration: 1000,
                    maxZoom: 15
                });
            }
        }
    }, [focusGeometry]);

    useEffect(() => {
        if (!mapRef.current) return;

        if (drawInteractionRef.current) {
            mapRef.current.removeInteraction(drawInteractionRef.current);
        }

        if (drawType !== 'None') {
            const newDrawInteraction = new Draw({
                source: vectorSourceRef.current,
                type: drawType,
            });
            drawInteractionRef.current = newDrawInteraction;
            mapRef.current.addInteraction(newDrawInteraction);

            newDrawInteraction.on('drawend', (event) => {
                if (event.feature.getGeometry()) {
                    onDrawEnd(event.feature.getGeometry()!);
                }
            });
        }

    }, [drawType, onDrawEnd]);

    return (
        <div ref={mapElement} style={{ width: '100%', height: '100%', position: 'relative' }}>
            {isMergeMode && selectedFeaturesForMerge.length === 2 && (
                <button
                    onClick={handleMerge}
                    style={{
                        position: 'absolute',
                        top: '10px',
                        left: '50%',
                        transform: 'translateX(-50%)',
                        zIndex: 1001,
                        padding: '10px 20px',
                        backgroundColor: 'rgba(0, 60, 136, 0.8)',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer',
                        boxShadow: '0 2px 4px rgba(0,0,0,0.2)'
                    }}
                >
                    Merge 2 selected polygons
                </button>
            )}
            <Popup content={popupContent} position={popupPosition} />
            <InfoPopup shape={selectedShape} containedShapes={containedShapes} onClose={handleCloseInfoPopup} position={infoPopupPosition} />
        </div>
    );
};

export default MapComponent;
