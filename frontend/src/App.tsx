import { useState, useEffect } from 'react';
import ShapeList from './components/ShapeList';
import MapComponent from './components/Map';
import Navbar from './components/Navbar';
import AddShapeModal from './components/AddShapeModal';
import ConfirmationModal from './components/ConfirmationModal';
import DeleteShapeModal from './components/DeleteShapeModal';
import './App.css';
import { Geometry } from 'ol/geom';
import GeoJSON from 'ol/format/GeoJSON';
import { getShapes, addShape, addShapes, deleteAllShapes, deleteShapeById, type Shape, type AddShape, type Geometry as ShapeGeometry } from './services/shapeService';
import logo from './assets/lk-amblem-1.png';

const createTestData = (): AddShape[] => {
  return [
    { name: 'Ankara', geometry: { type: 'Point', coordinates: [32.85, 39.93] } },
    { name: 'Istanbul', geometry: { type: 'Point', coordinates: [28.97, 41.00] } },
    { name: 'İzmir-Manisa Highway', geometry: { type: 'LineString', coordinates: [[27.14, 38.42], [27.43, 38.62]] } },
    { name: 'Antalya Coastline', geometry: { type: 'LineString', coordinates: [[30.71, 36.89], [30.9, 36.88]] } },
    { name: 'Göreme National Park', geometry: { type: 'Polygon', coordinates: [[[34.82, 38.64], [34.85, 38.64], [34.85, 38.66], [34.82, 38.66], [34.82, 38.64]]] } },
  ];
};

function App() {
  const [shapes, setShapes] = useState<Shape[]>([]);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
  const [isShapeListOpen, setIsShapeListOpen] = useState(false);
  const [shapeName, setShapeName] = useState('');
  const [drawType, setDrawType] = useState<'Point' | 'LineString' | 'Polygon' | 'None'>('None');
  const [drawnGeometry, setDrawnGeometry] = useState<Geometry | null>(null);
  const [refreshShapes, setRefreshShapes] = useState(false);
  const [shapeToDelete, setShapeToDelete] = useState<number | 'all' | null>(null);
  const [focusGeometry, setFocusGeometry] = useState<ShapeGeometry | null>(null);
  const [resetViewToggle, setResetViewToggle] = useState(false);

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

  const handleJumpToShape = (geometry: ShapeGeometry) => {
    setFocusGeometry(geometry);
  };

  const handleResetView = () => {
    setResetViewToggle(prev => !prev);
  };

  const handleCreateTestData = async () => {
    try {
      const testData = createTestData();
      await addShapes(testData);
      setRefreshShapes(prev => !prev);
    } catch (error) {
      console.error('Failed to create test data:', error);
    }
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
        onToggleShapeList={() => setIsShapeListOpen(!isShapeListOpen)}
        onResetViewClick={handleResetView}
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
      <div className="map-container">
        <MapComponent 
          shapes={shapes}
          drawType={drawType} 
          onDrawEnd={(geometry) => {
            setDrawnGeometry(geometry);
            setDrawType('None');
          }}
          focusGeometry={focusGeometry}
          resetViewToggle={resetViewToggle}
        />
        {isShapeListOpen && <ShapeList shapes={shapes} onJumpToShape={handleJumpToShape} />}
      </div>
      <div style={{ padding: '10px', textAlign: 'center', position: 'absolute', bottom: '10px', left: '50%', transform: 'translateX(-50%)', zIndex: 1000 }}>
        <button onClick={handleCreateTestData} style={{ backgroundColor: '#28a745', color: 'white', border: 'none', padding: '10px 20px', borderRadius: '5px', cursor: 'pointer', fontSize: '16px' }}>
          Create Test Data
        </button>
      </div>
    </div>
  );
}

export default App;
