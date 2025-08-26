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

const MapComponent: React.FC = () => {
    const mapElement = useRef<HTMLDivElement>(null);
    const [shapes, setShapes] = useState<Shape[]>([]);

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
        if (mapElement.current) {
            const map = new Map({
                target: mapElement.current,
                layers: [
                    new TileLayer({
                        source: new OSM(),
                    }),
                ],
                view: new View({
                    center: fromLonLat([35.2433, 38.9637]), // Center of Turkey
                    zoom: 5,
                }),
            });

            if (shapes.length > 0) {
                const vectorSource = new VectorSource({
                    features: new GeoJSON().readFeatures({
                        type: 'FeatureCollection',
                        features: shapes.map(shape => ({
                            type: 'Feature',
                            geometry: shape.geometry,
                            properties: {
                                name: shape.name,
                            },
                        })),
                    }, {
                        featureProjection: 'EPSG:3857', // Map projection
                    }),
                });

                const vectorLayer = new VectorLayer({
                    source: vectorSource,
                });

                map.addLayer(vectorLayer);
            }

            return () => {
                map.setTarget(undefined);
            };
        }
    }, [shapes]);

    return <div ref={mapElement} style={{ width: '1000px', height: '1000px' }} />;
};

export default MapComponent;
