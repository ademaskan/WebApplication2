import React from 'react';
import './Popup.css';

interface PopupProps {
    content: string;
    position: { x: number; y: number };
}

const Popup: React.FC<PopupProps> = ({ content, position }) => {
    if (!content) return null;

    return (
        <div
            className="popup"
            style={{ left: `${position.x}px`, top: `${position.y}px` }}
        >
            {content}
        </div>
    );
};

export default Popup;
