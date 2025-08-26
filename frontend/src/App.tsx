import ShapeList from './components/ShapeList';
import MapComponent from './components/Map';
import './App.css';

function App() {
  return (
    <div className="App">
      <h1>My Map</h1>
      <MapComponent />
      <ShapeList />
    </div>
  );
}

export default App;
