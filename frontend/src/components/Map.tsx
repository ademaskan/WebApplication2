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
import { getShapes, type Shape } from '../services/shapeService';
import Draw from 'ol/interaction/Draw';
import { Geometry } from 'ol/geom';

interface MapComponentProps {
    drawType: 'Point' | 'LineString' | 'Polygon' | 'None';
    onDrawEnd: (geometry: Geometry) => void;
}

const MapComponent: React.FC<MapComponentProps> = ({ drawType, onDrawEnd }) => {
    const mapElement = useRef<HTMLDivElement>(null);
    const mapRef = useRef<Map | null>(null);
    const [shapes, setShapes] = useState<Shape[]>([]);
    const drawInteractionRef = useRef<Draw | null>(null);
    const vectorSourceRef = useRef<VectorSource>(new VectorSource());

    useEffect(() => {
        const fetchShapes = async () => {
            try {
                const shapesData = await getShapes();
                setShapes(shapesData);
            } catch (error) {
                console.error('Failed to fetch shapes:', error);
            }
        };

        fetchShapes();
    }, []);

    useEffect(() => {
        if (mapElement.current && !mapRef.current) {
            const vectorSource = vectorSourceRef.current;

            const vectorLayer = new VectorLayer({
                source: vectorSource,
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
                    zoom: 5,
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
        <div ref={mapElement} style={{ width: '1000px', height: '1000px' }} />
    );
};

export default MapComponent;
