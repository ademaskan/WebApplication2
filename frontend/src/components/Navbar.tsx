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
    const [isExpanded, setIsExpanded] = useState(true);

    const navStyle: React.CSSProperties = {
        position: 'absolute',
        top: '20px',
        left: '20px',
        backgroundColor: 'rgba(255, 255, 255, 0.9)',
        borderRadius: '10px',
        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
        padding: '10px',
        zIndex: 1000,
        display: 'flex',
        flexDirection: 'column',
        gap: '10px',
        width: isExpanded ? '280px' : '40px',
        transition: 'width 0.3s ease',
        overflow: 'hidden'
    };

    const buttonStyle: React.CSSProperties = {
        backgroundColor: 'transparent',
        color: '#333',
        border: 'none',
        padding: '10px',
        borderRadius: '8px',
        cursor: 'pointer',
        display: 'flex',
        alignItems: 'center',
        gap: '10px',
        width: '100%',
        textAlign: 'left',
        transition: 'background-color 0.2s ease',
        fontSize: '14px'
    };
    
    const iconStyle: React.CSSProperties = {
        fontSize: '20px',
        minWidth: '20px',
        textAlign: 'center'
    };

    const hrStyle: React.CSSProperties = {
        border: 0,
        borderTop: '1px solid #eee',
        margin: '5px 0'
    };
    
    const searchInputStyle: React.CSSProperties = {
        padding: '8px',
        borderRadius: '5px',
        border: '1px solid #ccc',
        width: '100%',
        boxSizing: 'border-box'
    };

    return (
        <nav style={navStyle}>
            <button onClick={() => setIsExpanded(!isExpanded)} style={{ ...buttonStyle, justifyContent: 'center', padding: '5px', width: 'auto', alignSelf: 'flex-end' }}>
                <span style={iconStyle}>{isExpanded ? '‹' : '›'}</span>
            </button>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                <div style={{ position: 'relative' }}>
                    <input
                        type="text"
                        placeholder="Search by name..."
                        value={searchTerm}
                        onChange={(e) => onSearchChange(e.target.value)}
                        onFocus={() => setIsSearchFocused(true)}
                        onBlur={() => setTimeout(() => setIsSearchFocused(false), 200)}
                        style={searchInputStyle}
                    />
                    {isSearchFocused && searchTerm && (
                        <ul style={{ position: 'absolute', top: '100%', left: 0, right: 0, backgroundColor: 'white', border: '1px solid #ccc', borderRadius: '5px', listStyle: 'none', margin: '5px 0 0', padding: 0, zIndex: 1001, maxHeight: '150px', overflowY: 'auto' }}>
                            {filteredShapes.length > 0 ? filteredShapes.map(shape => (
                                <li key={shape.id} onClick={() => onJumpToShape(shape.geometry)} style={{ padding: '8px', cursor: 'pointer' }}>
                                    {shape.name}
                                </li>
                            )) : <li style={{ padding: '8px', color: '#888' }}>No results</li>}
                        </ul>
                    )}
                </div>
                
                <div style={{ display: 'flex', gap: '5px', justifyContent: 'space-around' }}>
                    <label title="Points"><input type="checkbox" checked={visibleTypes['Point']} onChange={(e) => onFilterChange('Point', e.target.checked)} /> Point</label>
                    <label title="Lines"><input type="checkbox" checked={visibleTypes['LineString']} onChange={(e) => onFilterChange('LineString', e.target.checked)} /> LineString</label>
                    <label title="Polygons"><input type="checkbox" checked={visibleTypes['Polygon']} onChange={(e) => onFilterChange('Polygon', e.target.checked)} /> Polygon</label>
                </div>
                <hr style={hrStyle} />
                <button onClick={onAddShapeClick} style={buttonStyle}><span style={iconStyle}>+</span> Add Shape</button>
                <button onClick={onSaveClick} style={{...buttonStyle, color: isSaveDisabled ? '#ccc' : '#333'}} disabled={isSaveDisabled}><span style={iconStyle}>✓</span> Save</button>
                <button onClick={onDeleteShapeClick} style={buttonStyle}><span style={iconStyle}>-</span> Delete Shape</button>
                <button onClick={onToggleMergeMode} style={{...buttonStyle, backgroundColor: isMergeMode ? '#e0e0e0' : 'transparent'}}><span style={iconStyle}>⧉</span> Merge Polygons</button>
                <hr style={hrStyle} />
                <button onClick={onToggleShapeList} style={buttonStyle}><span style={iconStyle}>≡</span> View Geometry</button>
                <button onClick={onResetViewClick} style={buttonStyle}><span style={iconStyle}>⟳</span> Reset View</button>
                <hr style={hrStyle} />
                <button onClick={onCreateTestDataClick} style={{...buttonStyle, color: '#28a745'}}><span style={iconStyle}>T</span> Create Test Data</button>
                <button onClick={onDeleteAllClick} style={{...buttonStyle, color: '#dc3545'}}><span style={iconStyle}>X</span> Delete All</button>
            </div>
        </nav>
    );
};

export default Navbar;
