import { useState, useEffect, useMemo } from 'react';
import ShapeList from './components/ShapeList';
import MapComponent from './components/Map';
import Navbar from './components/Navbar';
import AddShapeModal from './components/AddShapeModal';
import ConfirmationModal from './components/ConfirmationModal';
import DeleteShapeModal from './components/DeleteShapeModal';
import ErrorModal from './components/ErrorModal';
import CreateTestDataModal from './components/CreateTestDataModal';
import './App.css';
import { Geometry } from 'ol/geom';
import GeoJSON from 'ol/format/GeoJSON';
import {
    getShapes, addShape, addShapes, deleteAllShapes, deleteShapeById, mergeShapes, createTestData, updateShape,
    type Shape, type AddShape, type Geometry as ShapeGeometry, type MergeShapesRequest, type PagedResult
} from './services/shapeService';


function App() {
  const [shapes, setShapes] = useState<Shape[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
  const [isCreateTestDataModalOpen, setIsCreateTestDataModalOpen] = useState(false);
  const [isShapeListOpen, setIsShapeListOpen] = useState(false);
  const [shapeName, setShapeName] = useState('');
  const [shapeType, setShapeType] = useState<'A' | 'B' | 'C'>('A');
  const [drawType, setDrawType] = useState<'Point' | 'LineString' | 'Polygon' | 'None'>('None');
  const [drawnGeometry, setDrawnGeometry] = useState<Geometry | null>(null);
  const [imageFile, setImageFile] = useState<File | undefined>(undefined);
  const [refreshShapes, setRefreshShapes] = useState(false);
  const [shapeToDelete, setShapeToDelete] = useState<number | 'all' | null>(null);
  const [focusGeometry, setFocusGeometry] = useState<ShapeGeometry | null>(null);
  const [resetViewToggle, setResetViewToggle] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [isMergeMode, setIsMergeMode] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [isErrorModalOpen, setIsErrorModalOpen] = useState(false);
  const [clearLastDrawnFeature, setClearLastDrawnFeature] = useState(false);
  const [editingShape, setEditingShape] = useState<Shape | null>(null);
  const [modifiedGeometry, setModifiedGeometry] = useState<ShapeGeometry | null>(null);
  const [visibleTypes, setVisibleTypes] = useState<{ [key: string]: boolean }>({
    'Point': true,
    'LineString': true,
    'Polygon': true,
  });

  useEffect(() => {
    const fetchShapes = async () => {
      try {
        const result: PagedResult<Shape> = await getShapes(pageNumber, pageSize, searchTerm);
        setShapes(result.items);
        setTotalCount(result.totalCount);
        setTotalPages(result.totalPages);
      } catch (error) {
        console.error('Failed to fetch shapes:', error);
      }
    };

    fetchShapes();
  }, [refreshShapes, pageNumber, pageSize, searchTerm]);

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setPageNumber(1);
  };

  const handleStartDrawing = (name: string, geometryType: 'Point' | 'LineString' | 'Polygon', type: 'A' | 'B' | 'C', image?: File) => {
    setShapeName(name);
    setDrawType(geometryType);
    setShapeType(type);
    setImageFile(image);
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
        type: shapeType,
        image: imageFile,
      };

      try {
        await addShape(newShape);
        setDrawnGeometry(null);
        setShapeName('');
        setImageFile(undefined);
        setRefreshShapes(prev => !prev);
      } catch (error) {
        console.error('Failed to save shape:', error);
        setErrorMessage(error instanceof Error ? error.message : 'An unknown error occurred.');
        setIsErrorModalOpen(true);
        setDrawnGeometry(null);
        setClearLastDrawnFeature(prev => !prev);
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
      type: 'A',
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
    setPageNumber(1);
  };

  const onUpdateShape = async (id: number, newName: string) => {
    const originalShape = shapes.find(s => s.id === id);
    if (!originalShape) return;

    const updatePayload = {
      newName: newName,
      newGeometry: modifiedGeometry || undefined
    };

    try {
      await updateShape(id, updatePayload);
      setEditingShape(null);
      setModifiedGeometry(null);
      setRefreshShapes(prev => !prev);
    } catch (error) {
      console.error('Failed to update shape:', error);
      setErrorMessage(error instanceof Error ? error.message : 'An unknown error occurred.');
      setIsErrorModalOpen(true);
    }
  };

  const filteredShapes = useMemo(() => {
    const types = Object.keys(visibleTypes).filter(key => visibleTypes[key]);
    
    return (shapes || [])
      .filter(shape => shape && shape.geometry && types.includes(shape.geometry.type));
      
  }, [shapes, visibleTypes]);

  const handleGenerateTestData = async (count: number) => {
    try {
      await createTestData(count);
      setRefreshShapes(prev => !prev);
      setIsCreateTestDataModalOpen(false);
    } catch (error) {
      console.error('Failed to create test data:', error);
      setErrorMessage(error instanceof Error ? error.message : 'An unknown error occurred.');
      setIsErrorModalOpen(true);
    }
  };

  const handleCreateTestData = () => {
    setIsCreateTestDataModalOpen(true);
  };

  return (
    <div className="App">
      
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
            <div className="pagination-controls">
                <button onClick={() => setPageNumber(prev => Math.max(prev - 1, 1))} disabled={pageNumber === 1 || pageSize <= 0}>
                    Previous
                </button>
                <span>Page {pageNumber} of {totalPages}</span>
                <button onClick={() => setPageNumber(prev => Math.min(prev + 1, totalPages))} disabled={pageNumber === totalPages || pageSize <= 0}>
                    Next
                </button>
                <select value={pageSize} onChange={(e) => handlePageSizeChange(Number(e.target.value))}>
                    <option value="10">10</option>
                    <option value="25">25</option>
                    <option value="50">50</option>
                    <option value="100">100</option>
                    <option value="500">500</option>
                    <option value="1000">1,000</option>
                    <option value="10000">10,000</option>
                    <option value="50000">50,000</option>
                    <option value="100000">100,000</option>
                    <option value="500000">500,000</option>
                    <option value="-1">All</option>
                </select>
            </div>
      <AddShapeModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        onStartDrawing={handleStartDrawing}
      />
      <CreateTestDataModal
        isOpen={isCreateTestDataModalOpen}
        onClose={() => setIsCreateTestDataModalOpen(false)}
        onGenerate={handleGenerateTestData}
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
      <ErrorModal
        isOpen={isErrorModalOpen}
        onClose={() => setIsErrorModalOpen(false)}
        message={errorMessage}
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
          clearLastDrawnFeature={clearLastDrawnFeature}
          editingShape={editingShape}
          onShapeModified={(geometry) => setModifiedGeometry(geometry)}
          onUpdateShape={onUpdateShape}
          onDeleteShape={handleDeleteRequest}
          setEditingShape={setEditingShape}
        />
        {isShapeListOpen && <ShapeList shapes={filteredShapes} onJumpToShape={handleJumpToShape} onClose={() => setIsShapeListOpen(false)} />}
      </div>
    </div>
  );
}

export default App;
