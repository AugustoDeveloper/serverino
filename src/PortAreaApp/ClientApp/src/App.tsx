import React from 'react';
import {HashRouter, Redirect, Route} from 'react-router-dom'
import './App.css';
import Home from "./Home";

function App() {
  return (
  <HashRouter basename="/">
      <Route exact path="/">
          <Redirect to="/app/" />
      </Route>
      <Route path="/app">
          <Home />
      </Route>
  </HashRouter>
  );
}

export default App;
