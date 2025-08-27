import React from 'react';

interface NavbarProps {
    onAddShapeClick: () => void;
    onSaveClick: () => void;
    isSaveDisabled: boolean;
    onDeleteAllClick: () => void;
    onDeleteShapeClick: () => void;
    onToggleShapeList: () => void;
    onResetViewClick: () => void;
}

const Navbar: React.FC<NavbarProps> = ({ onAddShapeClick, onSaveClick, isSaveDisabled, onDeleteAllClick, onDeleteShapeClick, onToggleShapeList, onResetViewClick }) => {
    const buttonStyle: React.CSSProperties = {
        backgroundColor: '#0056b3',
        color: 'white',
        border: 'none',
        padding: '10px 15px',
        borderRadius: '5px',
        cursor: 'pointer',
    };

    const disabledButtonStyle: React.CSSProperties = {
        ...buttonStyle,
        backgroundColor: '#ccc',
        cursor: 'not-allowed',
    };

    return (
        <nav style={{ padding: '10px', backgroundColor: '#f8f9fa', display: 'flex', gap: '10px', borderBottom: '1px solid #ddd' }}>
            <button onClick={onAddShapeClick} style={buttonStyle}>Add Shape</button>
            <button onClick={onSaveClick} style={isSaveDisabled ? disabledButtonStyle : buttonStyle} disabled={isSaveDisabled}>Save</button>
            <button onClick={onDeleteShapeClick} style={buttonStyle}>Delete Shape</button>
            <button onClick={onDeleteAllClick} style={buttonStyle}>Delete All</button>
            <button onClick={onToggleShapeList} style={buttonStyle}>Toggle Shapes</button>
            <button onClick={onResetViewClick} style={buttonStyle}>Reset View</button>
        </nav>
    );
};

export default Navbar;
