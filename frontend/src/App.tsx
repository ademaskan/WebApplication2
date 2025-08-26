import React, { useState } from 'react';
import ShapeList from './components/ShapeList';
import MapComponent from './components/Map';
import Navbar from './components/Navbar';
import AddShapeModal from './components/AddShapeModal';
import './App.css';
import { Geometry } from 'ol/geom';
import GeoJSON from 'ol/format/GeoJSON';
import { addShape, deleteAllShapes, type AddShape } from './services/shapeService';

function App() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [shapeName, setShapeName] = useState('');
  const [drawType, setDrawType] = useState<'Point' | 'LineString' | 'Polygon' | 'None'>('None');
  const [drawnGeometry, setDrawnGeometry] = useState<Geometry | null>(null);
  const [refreshShapes, setRefreshShapes] = useState(false);

  const handleStartDrawing = (name: string, type: 'Point' | 'LineString' | 'Polygon') => {
    setShapeName(name);
    setDrawType(type);
    setDrawnGeometry(null); 
  };
  
  const handleSave = async () => {
    if (drawnGeometry && shapeName) {
      const geoJsonFormat = new GeoJSON({
        featureProjection: 'EPSG:3857',
        dataProjection: 'EPSG:4326'
      });
      const geoJsonGeom = geoJsonFormat.writeGeometryObject(drawnGeometry);

      const newShape: AddShape = {
        name: shapeName,
        geometry: geoJsonGeom,
      };

      try {
        await addShape(newShape);
        setDrawnGeometry(null);
        setShapeName('');
        setRefreshShapes(prev => !prev); // Trigger refresh
      } catch (error) {
        console.error('Failed to save shape:', error);
      }
    }
  };

  const handleDeleteAll = async () => {
    try {
      await deleteAllShapes();
      setRefreshShapes(prev => !prev); // Trigger refresh
    } catch (error) {
      console.error('Failed to delete all shapes:', error);
    }
  };

  return (
    <div className="App">
      <Navbar 
        onAddShapeClick={() => setIsModalOpen(true)} 
        onSaveClick={handleSave} 
        isSaveDisabled={!drawnGeometry}
        onDeleteAllClick={handleDeleteAll} 
      />
      <AddShapeModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onStartDrawing={handleStartDrawing}
      />
      <div style={{ display: 'flex' }}>
        <MapComponent 
          key={`map-${refreshShapes}`}
          drawType={drawType} 
          onDrawEnd={(geometry) => {
            setDrawnGeometry(geometry);
            setDrawType('None');
          }}
        />
        <ShapeList key={`list-${refreshShapes}`} /> 
      </div>
    </div>
  );
}

export default App;
