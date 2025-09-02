import { Style, Icon, Stroke, Fill, Circle as CircleStyle } from 'ol/style';
import MultiPoint from 'ol/geom/MultiPoint';
import { type LineString, type Polygon } from 'ol/geom';
import pinIconRed from '../assets/placeholder.png';
import pinIconGreen from '../assets/placeholder-green.png';
import pinIconBlue from '../assets/placeholder-blue.png';

const typeColors: { [key: string]: string } = {
    'A': '#ff0000', // Red
    'B': '#00ff00', // Green
    'C': '#0000ff', // Blue
};

const typeIcons: { [key: string]: string } = {
    'A': pinIconRed,
    'B': pinIconGreen,
    'C': pinIconBlue,
};

export const pointStyle = (type: string) => new Style({
    image: new Icon({
        anchor: [0.5, 1],
        src: typeIcons[type] || pinIconRed,
        scale: 0.07,
    }),
});

export const lineStringStyle = (type: string) => new Style({
    stroke: new Stroke({
        color: typeColors[type] || '#0056b3',
        width: 3,
    }),
});

export const polygonStyle = (type: string) => new Style({
    stroke: new Stroke({
        color: typeColors[type] || '#0056b3',
        width: 2,
    }),
    fill: new Fill({
        color: `${typeColors[type] || '#0056b3'}33`, // Adding alpha for fill
    }),
});

export const vertexStyle = new Style({
    image: new CircleStyle({
        radius: 5,
        fill: new Fill({ color: 'orange' }),
    }),
    geometry: (feature) => {
        const geom = feature.getGeometry();
        if (!geom) return;

        const type = geom.getType();
        if (type === 'Polygon') {
            // Use the coordinates of the first ring of the polygon
            const coordinates = (geom as Polygon).getCoordinates()[0];
            return new MultiPoint(coordinates);
        } else if (type === 'LineString') {
            const coordinates = (geom as LineString).getCoordinates();
            return new MultiPoint(coordinates);
        }
    },
});

export const styleFunction = (feature: any) => {
    const geometry = feature.getGeometry();
    if (!geometry) return;

    const geometryType = geometry.getType();
    const type = feature.get('type') || 'A';
    switch (geometryType) {
        case 'Point':
            return pointStyle(type);
        case 'LineString':
            return [lineStringStyle(type), vertexStyle];
        case 'Polygon':
            return [polygonStyle(type), vertexStyle];
        default:
            return new Style({
                stroke: new Stroke({
                    color: 'grey',
                    width: 2
                }),
                fill: new Fill({
                    color: 'rgba(128,128,128,0.2)'
                })
            });
    }
};
