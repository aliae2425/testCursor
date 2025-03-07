import React, { useEffect, useRef } from 'react';

/* global Autodesk */

export function ForgeViewer({ accessToken, urn }) {
  const viewerRef = useRef(null);
  const containerRef = useRef(null);

  useEffect(() => {
    if (!accessToken || !urn) {
      return;
    }

    const options = {
      env: 'AutodeskProduction',
      accessToken: accessToken
    };

    Autodesk.Viewing.Initializer(options, () => {
      const viewer = new Autodesk.Viewing.GuiViewer3D(containerRef.current);
      viewer.start();
      viewer.loadDocumentNode(urn).then(function(doc) {
        // Le modèle est chargé
      }).catch(function(err) {
        console.error('Erreur lors du chargement du modèle:', err);
      });

      viewerRef.current = viewer;
    });

    return () => {
      if (viewerRef.current) {
        viewerRef.current.finish();
      }
    };
  }, [accessToken, urn]);

  return (
    <div 
      ref={containerRef} 
      style={{ 
        width: '100%', 
        height: '400px', 
        backgroundColor: '#1a1d23',
        borderRadius: '10px',
        overflow: 'hidden'
      }} 
    />
  );
} 