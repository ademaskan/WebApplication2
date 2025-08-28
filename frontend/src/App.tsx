import { useState, useEffect, useMemo } from 'react';
import ShapeList from './components/ShapeList';
import MapComponent from './components/Map';
import Navbar from './components/Navbar';
import AddShapeModal from './components/AddShapeModal';
import ConfirmationModal from './components/ConfirmationModal';
import DeleteShapeModal from './components/DeleteShapeModal';
import './App.css';
import { Geometry } from 'ol/geom';
import GeoJSON from 'ol/format/GeoJSON';
import {
    getShapes, addShape, addShapes, deleteAllShapes, deleteShapeById, mergeShapes,
    type Shape, type AddShape, type Geometry as ShapeGeometry, type MergeShapesRequest
} from './services/shapeService';
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
  const [searchTerm, setSearchTerm] = useState('');
  const [isMergeMode, setIsMergeMode] = useState(false);
  const [visibleTypes, setVisibleTypes] = useState<{ [key: string]: boolean }>({
    'Point': true,
    'LineString': true,
    'Polygon': true,
  });

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
        geometry: geoJsonGeom as ShapeGeometry,
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

  const handleMergeShapes = async (name: string, geometry: ShapeGeometry, deleteIds: number[]) => {
    const request: MergeShapesRequest = {
      name,
      geometry,
      deleteIds
    };
    console.log("handleMergeShapes called with request:", request);

    try {
      await mergeShapes(request);
      console.log("Merge successful, refreshing shapes.");
      setRefreshShapes(prev => !prev);
      setIsMergeMode(false);
    } catch (error) {
      console.error('Failed to merge shapes:', error);
      alert('Failed to merge shapes. See console for details.');
    }
  };
  
  const toggleMergeMode = () => {
    setIsMergeMode(prev => !prev);
  };

  const handleFilterChange = (type: string, isVisible: boolean) => {
    setVisibleTypes(prev => ({ ...prev, [type]: isVisible }));
  };

  const handleSearchChange = (term: string) => {
    setSearchTerm(term);
  };

  const filteredShapes = useMemo(() => {
    const types = Object.keys(visibleTypes).filter(key => visibleTypes[key]);
    
    return shapes
      .filter(shape => types.includes(shape.geometry.type))
      .filter(shape => shape.name.toLowerCase().includes(searchTerm.toLowerCase()));
      
  }, [shapes, visibleTypes, searchTerm]);

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
        onCreateTestDataClick={handleCreateTestData}
        onToggleMergeMode={toggleMergeMode}
        isMergeMode={isMergeMode}
        visibleTypes={visibleTypes}
        onFilterChange={handleFilterChange}
        searchTerm={searchTerm}
        onSearchChange={handleSearchChange}
        filteredShapes={filteredShapes}
        onJumpToShape={handleJumpToShape}
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
          shapes={filteredShapes}
          drawType={drawType} 
          onDrawEnd={(geometry) => {
            setDrawnGeometry(geometry);
            setDrawType('None');
          }}
          focusGeometry={focusGeometry}
          resetViewToggle={resetViewToggle}
          isMergeMode={isMergeMode}
          onMerge={handleMergeShapes}
        />
        {isShapeListOpen && <ShapeList shapes={filteredShapes} onJumpToShape={handleJumpToShape} onClose={() => setIsShapeListOpen(false)} />}
      </div>
    </div>
  );
}

export default App;
