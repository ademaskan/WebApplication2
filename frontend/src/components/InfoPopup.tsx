import React from 'react';
import './Popup.css';
import { type Shape } from '../services/shapeService';

interface InfoPopupProps {
    shape: Shape | null;
    onClose: () => void;
    position: { x: number; y: number } | null;
}

const InfoPopup: React.FC<InfoPopupProps> = ({ shape, onClose, position }) => {
    if (!shape || !position) {
        return null;
    }

    const style: React.CSSProperties = {
        position: 'absolute',
        left: `${position.x}px`,
        top: `${position.y}px`,
        transform: 'translate(-50%, calc(-100% - 10px))'
    };

    return (
        <div className="info-popup-content" style={style}>
            <button onClick={onClose} className="info-popup-close-btn">&times;</button>
            <div className="info-popup-header">Shape Details</div>
            <div className="info-popup-body">
                <p><strong>ID:</strong> {shape.id}</p>
                <p><strong>Name:</strong> {shape.name}</p>
                <p><strong>WKT:</strong> <span className="wkt-text">{shape.wkt}</span></p>
            </div>
        </div>
    );
};

export default InfoPopup;
