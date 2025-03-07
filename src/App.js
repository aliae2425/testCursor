import React, { useState } from 'react';
import './App.css';
import { ForgeViewer } from './components/ForgeViewer';
import { getAccessToken, uploadModel } from './services/forgeService';

function App() {
  const [accessToken, setAccessToken] = useState(null);
  const [urn, setUrn] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const fileInfo = {
    name: '418_TeaPot.rfa',
    type: 'Fichier Revit Family (RFA)',
    size: '472 KB',
    lastModified: '7 mars 2024'
  };

  const handleFileUpload = async () => {
    try {
      setIsLoading(true);
      
      // Obtenir le fichier
      const file = new File([], fileInfo.name, {
        type: 'application/octet-stream'
      });
      
      // Upload et traduire le modèle
      const modelUrn = await uploadModel(file);
      
      // Obtenir un token d'accès
      const token = await getAccessToken();
      
      setAccessToken(token);
      setUrn(modelUrn);
    } catch (error) {
      console.error('Erreur:', error);
      alert('Une erreur est survenue lors de la configuration. Veuillez vérifier vos identifiants Forge.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Prévisualisation du fichier</h1>
        <div className="file-info-container">
          <div className="file-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" width="48" height="48">
              <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"></path>
              <polyline points="13 2 13 9 20 9"></polyline>
            </svg>
          </div>
          <div className="file-details">
            <h2>{fileInfo.name}</h2>
            <table>
              <tbody>
                <tr>
                  <td>Type:</td>
                  <td>{fileInfo.type}</td>
                </tr>
                <tr>
                  <td>Taille:</td>
                  <td>{fileInfo.size}</td>
                </tr>
                <tr>
                  <td>Dernière modification:</td>
                  <td>{fileInfo.lastModified}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        
        <div className="viewer-container">
          {accessToken && urn ? (
            <ForgeViewer 
              accessToken={accessToken} 
              urn={urn}
            />
          ) : (
            <div className="auth-message">
              <p>Pour voir le modèle 3D, vous devez :</p>
              <ol>
                <li>Créer un compte sur <a href="https://forge.autodesk.com/" target="_blank" rel="noopener noreferrer">Autodesk Platform Services</a></li>
                <li>Créer une application pour obtenir les clés API</li>
                <li>Configurer les variables d'environnement :
                  <pre>
                    REACT_APP_FORGE_CLIENT_ID=votre_client_id
                    REACT_APP_FORGE_CLIENT_SECRET=votre_client_secret
                  </pre>
                </li>
              </ol>
              <button 
                className="auth-button"
                onClick={handleFileUpload}
                disabled={isLoading}
              >
                {isLoading ? 'Configuration en cours...' : 'Configurer l\'accès Forge'}
              </button>
            </div>
          )}
        </div>

        <p className="note">
          Note: Pour visualiser ce fichier Revit, nous utilisons Autodesk Platform Services.
          Veuillez configurer vos identifiants API pour activer la prévisualisation 3D.
        </p>
      </header>
    </div>
  );
}

export default App; 