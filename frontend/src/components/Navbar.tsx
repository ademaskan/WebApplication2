import React, { useState } from 'react';
import { type Shape } from '../services/shapeService';

interface NavbarProps {
    onAddShapeClick: () => void;
    onSaveClick: () => void;
    isSaveDisabled: boolean;
    onDeleteAllClick: () => void;
    onDeleteShapeClick: () => void;
    onToggleShapeList: () => void;
    onResetViewClick: () => void;
    onCreateTestDataClick: () => void;
    onToggleMergeMode: () => void;
    isMergeMode: boolean;
    visibleTypes: { [key: string]: boolean };
    onFilterChange: (type: string, isVisible: boolean) => void;
    searchTerm: string;
    onSearchChange: (term: string) => void;
    filteredShapes: Shape[];
    onJumpToShape: (geometry: any) => void;
}

const Navbar: React.FC<NavbarProps> = ({ 
    onAddShapeClick, onSaveClick, isSaveDisabled, onDeleteAllClick, 
    onDeleteShapeClick, onToggleShapeList, onResetViewClick, 
    onCreateTestDataClick, visibleTypes, onFilterChange, 
    searchTerm, onSearchChange, filteredShapes, onJumpToShape,
    onToggleMergeMode, isMergeMode
}) => {
    const [isSearchFocused, setIsSearchFocused] = useState(false);

    const buttonStyle: React.CSSProperties = {
        backgroundColor: '#0056b3',
        color: 'white',
        border: 'none',
        padding: '10px 15px',
        borderRadius: '5px',
        cursor: 'pointer',
    };
    
    const activeButtonSyle: React.CSSProperties = {
        ...buttonStyle,
        backgroundColor: '#004080',
    };

    const disabledButtonStyle: React.CSSProperties = {
        ...buttonStyle,
        backgroundColor: '#ccc',
        cursor: 'not-allowed',
    };

    return (
        <nav style={{ padding: '10px', backgroundColor: '#f8f9fa', display: 'flex', gap: '10px', borderBottom: '1px solid #ddd', alignItems: 'center' }}>
            <button onClick={onAddShapeClick} style={buttonStyle}>Add Shape</button>
            <button onClick={onSaveClick} style={isSaveDisabled ? disabledButtonStyle : buttonStyle} disabled={isSaveDisabled}>Save</button>
            <button onClick={onDeleteShapeClick} style={buttonStyle}>Delete Shape</button>
            <button onClick={onResetViewClick} style={buttonStyle}>Reset View</button>
            <button onClick={onToggleMergeMode} style={isMergeMode ? activeButtonSyle : buttonStyle}>Merge Polygons</button>

            <div style={{ marginLeft: 'auto', display: 'flex', gap: '10px', alignItems: 'center' }}>
                <button onClick={onDeleteAllClick} style={{...buttonStyle, backgroundColor: '#dc3545'}}>Delete All</button>
                <button onClick={onCreateTestDataClick} style={{...buttonStyle, backgroundColor: '#28a745'}}>Create Test Data</button>
                <button onClick={onToggleShapeList} style={buttonStyle}>View Geometry</button>
                
                <div style={{ display: 'flex', gap: '10px', alignItems: 'center', borderLeft: '1px solid #ddd', paddingLeft: '10px' }}>
                    <label><input type="checkbox" checked={visibleTypes['Point']} onChange={(e) => onFilterChange('Point', e.target.checked)} /> Points</label>
                    <label><input type="checkbox" checked={visibleTypes['LineString']} onChange={(e) => onFilterChange('LineString', e.target.checked)} /> Lines</label>
                    <label><input type="checkbox" checked={visibleTypes['Polygon']} onChange={(e) => onFilterChange('Polygon', e.target.checked)} /> Polygons</label>
                </div>
                <div style={{ position: 'relative', display: 'flex', alignItems: 'center', borderLeft: '1px solid #ddd', paddingLeft: '10px' }}>
                    <input
                        type="text"
                        placeholder="Search by name..."
                        value={searchTerm}
                        onChange={(e) => onSearchChange(e.target.value)}
                        onFocus={() => setIsSearchFocused(true)}
                        onBlur={() => setTimeout(() => setIsSearchFocused(false), 100)}
                        style={{ padding: '8px', borderRadius: '5px', border: '1px solid #ccc', paddingRight: '25px' }}
                    />
                    {searchTerm && (
                        <button 
                            onClick={() => onSearchChange('')} 
                            style={{ position: 'absolute', right: '5px', top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', fontSize: '16px', color: '#888' }}
                        >
                            &times;
                        </button>
                    )}
                    {isSearchFocused && searchTerm && (
                        <ul style={{ position: 'absolute', top: '100%', left: 0, right: 0, backgroundColor: 'white', border: '1px solid #ccc', borderRadius: '5px', listStyle: 'none', margin: 0, padding: 0, zIndex: 1000 }}>
                            {filteredShapes.map(shape => (
                                <li 
                                    key={shape.id} 
                                    onClick={() => onJumpToShape(shape.geometry)}
                                    style={{ padding: '8px', cursor: 'pointer' }}
                                >
                                    {shape.name}
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            </div>
        </nav>
    );
};

export default Navbar;
