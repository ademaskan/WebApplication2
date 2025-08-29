import { Style, Icon, Stroke, Fill } from 'ol/style';
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

export const styleFunction = (feature: any) => {
    const geometryType = feature.getGeometry()?.getType();
    const type = feature.get('type') || 'A';
    switch (geometryType) {
        case 'Point':
            return pointStyle(type);
        case 'LineString':
            return lineStringStyle(type);
        case 'Polygon':
            return polygonStyle(type);
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
