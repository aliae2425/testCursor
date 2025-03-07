import React from 'react';
import { Canvas } from '@react-three/fiber';
import { OrbitControls, Stage } from '@react-three/drei';
import { Teapot } from './Teapot';

export function TeapotPreview() {
  return (
    <div style={{ width: '300px', height: '300px', backgroundColor: '#1a1d23', borderRadius: '10px', overflow: 'hidden' }}>
      <Canvas camera={{ position: [5, 5, 5], fov: 45 }}>
        <Stage environment="city" intensity={0.5}>
          <Teapot />
        </Stage>
        <OrbitControls autoRotate />
      </Canvas>
    </div>
  );
} 