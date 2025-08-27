import { Style, Icon, Stroke, Fill } from 'ol/style';
import pinIcon from '../assets/placeholder.png';

export const pointStyle = new Style({
    image: new Icon({
        anchor: [0.5, 1],
        src: pinIcon,
        scale: 0.07,
    }),
});

export const lineStringStyle = new Style({
    stroke: new Stroke({
        color: '#0056b3',
        width: 3,
    }),
});

export const polygonStyle = new Style({
    stroke: new Stroke({
        color: '#0056b3',
        width: 2,
    }),
    fill: new Fill({
        color: 'rgba(0, 86, 179, 0.2)',
    }),
});

export const styleFunction = (feature: any) => {
    const geometryType = feature.getGeometry()?.getType();
    switch (geometryType) {
        case 'Point':
            return pointStyle;
        case 'LineString':
            return lineStringStyle;
        case 'Polygon':
            return polygonStyle;
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
