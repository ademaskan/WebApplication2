import { Style, Icon, Stroke, Fill } from 'ol/style';
import pinIcon from '../assets/placeholder.png';

const typeColors: { [key: string]: string } = {
    'A': '#ff0000', // Red
    'B': '#00ff00', // Green
    'C': '#0000ff', // Blue
};

export const pointStyle = (type: string) => new Style({
    image: new Icon({
        anchor: [0.5, 1],
        src: pinIcon,
        scale: 0.07,
        color: typeColors[type] || '#0056b3',
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
