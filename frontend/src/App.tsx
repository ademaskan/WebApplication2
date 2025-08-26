import React, { useState, useEffect } from 'react';
import ShapeList from './components/ShapeList';
import MapComponent from './components/Map';
import Navbar from './components/Navbar';
import AddShapeModal from './components/AddShapeModal';
import ConfirmationModal from './components/ConfirmationModal';
import DeleteShapeModal from './components/DeleteShapeModal';
import './App.css';
import { Geometry } from 'ol/geom';
import GeoJSON from 'ol/format/GeoJSON';
import { getShapes, addShape, deleteAllShapes, deleteShapeById, type Shape, type AddShape } from './services/shapeService';
import logo from './assets/lk-amblem-1.png';

function App() {
  const [shapes, setShapes] = useState<Shape[]>([]);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
  const [shapeName, setShapeName] = useState('');
  const [drawType, setDrawType] = useState<'Point' | 'LineString' | 'Polygon' | 'None'>('None');
  const [drawnGeometry, setDrawnGeometry] = useState<Geometry | null>(null);
  const [refreshShapes, setRefreshShapes] = useState(false);
  const [shapeToDelete, setShapeToDelete] = useState<number | 'all' | null>(null);

  useEffect(() => {
    const fetchShapes = async () => {
      try {
        const shapesData = await getShapes();
        setShapes(shapesData);
      } catch (error) {
        console.error('Failed to fetch shapes:', error);
      }
    };

    fetchShapes();
  }, [refreshShapes]);

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
        setRefreshShapes(prev => !prev);
      } catch (error) {
        console.error('Failed to save shape:', error);
      }
    }
  };

  const handleDeleteRequest = (id: number | 'all') => {
    setShapeToDelete(id);
    setIsConfirmModalOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (shapeToDelete === null) return;

    try {
      if (shapeToDelete === 'all') {
        await deleteAllShapes();
      } else {
        await deleteShapeById(shapeToDelete);
      }
      setRefreshShapes(prev => !prev);
    } catch (error) {
      console.error('Failed to delete:', error);
    }
    setShapeToDelete(null);
  };

  return (
    <div className="App">
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '10px', backgroundColor: '#f8f9fa' }}>
        <img src={logo} alt="Başarsoft Logo" style={{ height: '40px', marginRight: '15px' }} />
        <h1 style={{ color: '#0056b3', margin: 0 }}>Başarsoft WKT Map</h1>
      </div>
      <Navbar 
        onAddShapeClick={() => setIsAddModalOpen(true)} 
        onSaveClick={handleSave} 
        isSaveDisabled={!drawnGeometry}
        onDeleteAllClick={() => handleDeleteRequest('all')}
        onDeleteShapeClick={() => setIsDeleteModalOpen(true)}
      />
      <AddShapeModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        onStartDrawing={handleStartDrawing}
      />
      <DeleteShapeModal
        isOpen={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        shapes={shapes}
        onDelete={(id) => handleDeleteRequest(id)}
      />
      <ConfirmationModal
        isOpen={isConfirmModalOpen}
        onClose={() => setIsConfirmModalOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Confirm Deletion"
        message={`Are you sure you want to delete ${shapeToDelete === 'all' ? 'all shapes' : 'this shape'}? This action cannot be undone.`}
      />
      <div style={{ display: 'flex' }}>
        <MapComponent 
          shapes={shapes}
          drawType={drawType} 
          onDrawEnd={(geometry) => {
            setDrawnGeometry(geometry);
            setDrawType('None');
          }}
        />
        <ShapeList shapes={shapes} />
      </div>
    </div>
  );
}

export default App;
