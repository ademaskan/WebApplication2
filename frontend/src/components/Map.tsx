import React, { useEffect, useRef } from 'react';
import 'ol/ol.css';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import VectorLayer from 'ol/layer/Vector';
import OSM from 'ol/source/OSM';
import VectorSource from 'ol/source/Vector';
import GeoJSON from 'ol/format/GeoJSON';
import { fromLonLat } from 'ol/proj';
import { type Shape, type Geometry as ShapeGeometry } from '../services/shapeService';
import Draw from 'ol/interaction/Draw';
import { Geometry } from 'ol/geom';
import { styleFunction } from './style';

interface MapComponentProps {
    shapes: Shape[];
    drawType: 'Point' | 'LineString' | 'Polygon' | 'None';
    onDrawEnd: (geometry: Geometry) => void;
    focusGeometry?: ShapeGeometry | null;
    resetViewToggle: boolean;
    onFeatureClick: (geometry: ShapeGeometry) => void;
}

const MapComponent: React.FC<MapComponentProps> = ({ shapes, drawType, onDrawEnd, focusGeometry, resetViewToggle, onFeatureClick }) => {
    const mapElement = useRef<HTMLDivElement>(null);
    const mapRef = useRef<Map | null>(null);
    const drawInteractionRef = useRef<Draw | null>(null);
    const vectorSourceRef = useRef<VectorSource>(new VectorSource());

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
        if (!mapRef.current) return;
        const map = mapRef.current;

        const handleClick = (event: any) => {
            map.forEachFeatureAtPixel(event.pixel, (feature: any) => {
                const geojson = new GeoJSON().writeFeatureObject(feature, {
                    dataProjection: 'EPSG:4326',
                    featureProjection: 'EPSG:3857'
                });
                onFeatureClick(geojson.geometry);
                return true;
            });
        };

        map.on('click', handleClick);

        return () => {
            map.un('click', handleClick);
        };
    }, [onFeatureClick]);

    useEffect(() => {
        if (resetViewToggle && mapRef.current) {
            mapRef.current.getView().animate({
                center: fromLonLat([35.2433, 38.9637]),
                zoom: 5,
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
        <div ref={mapElement} style={{ width: '100%', height: '100%' }} />
    );
};

export default MapComponent;
