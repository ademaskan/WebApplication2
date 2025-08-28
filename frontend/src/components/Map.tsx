import React, { useEffect, useRef, useState } from 'react';
import 'ol/ol.css';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import VectorLayer from 'ol/layer/Vector';
import OSM from 'ol/source/OSM';
import VectorSource from 'ol/source/Vector';
import GeoJSON from 'ol/format/GeoJSON';
import { fromLonLat } from 'ol/proj';
import Popup from './Popup';
import InfoPopup from './InfoPopup';
import { type Shape, type Geometry as ShapeGeometry } from '../services/shapeService';
import Draw from 'ol/interaction/Draw';
import { Geometry } from 'ol/geom';
import OLPolygon from 'ol/geom/Polygon';
import SimpleGeometry from 'ol/geom/SimpleGeometry';
import { styleFunction } from './style';
import { getCenter } from 'ol/extent';

interface MapComponentProps {
    shapes: Shape[];
    drawType: 'Point' | 'LineString' | 'Polygon' | 'None';
    onDrawEnd: (geometry: Geometry) => void;
    focusGeometry?: ShapeGeometry | null;
    resetViewToggle: boolean;
}

const MapComponent: React.FC<MapComponentProps> = ({ shapes, drawType, onDrawEnd, focusGeometry, resetViewToggle }) => {
    const mapElement = useRef<HTMLDivElement>(null);
    const mapRef = useRef<Map | null>(null);
    const drawInteractionRef = useRef<Draw | null>(null);
    const vectorSourceRef = useRef<VectorSource>(new VectorSource());
    const [popupContent, setPopupContent] = useState('');
    const [popupPosition, setPopupPosition] = useState({ x: 0, y: 0 });
    const [selectedShape, setSelectedShape] = useState<Shape | null>(null);
    const [containedShapes, setContainedShapes] = useState<Shape[]>([]);
    const [infoPopupPosition, setInfoPopupPosition] = useState<{ x: number, y: number } | null>(null);

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
        const map = mapRef.current;

        const handleClick = (event: any) => {
            if (drawType !== 'None') return;

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
                                
                                if (otherGeom.getType() === 'Point' || otherGeom.getType() === 'LineString') {
                                    const coordinates = (otherGeom as SimpleGeometry).getCoordinates();
                                    
                                    let isContained = false;
                                    if (otherGeom.getType() === 'Point') {
                                        if (coordinates && polygonGeom.intersectsCoordinate(coordinates as number[])) {
                                            isContained = true;
                                        }
                                    } else { // LineString
                                        isContained = !!coordinates && (coordinates as number[][]).every(coord => polygonGeom.intersectsCoordinate(coord));
                                    }

                                    if (isContained) {
                                        contained.push(otherShape);
                                    }
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
    }, [drawType, shapes]);

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
            <Popup content={popupContent} position={popupPosition} />
            <InfoPopup shape={selectedShape} containedShapes={containedShapes} onClose={handleCloseInfoPopup} position={infoPopupPosition} />
        </div>
    );
};

export default MapComponent;
