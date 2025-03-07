import React from 'react';
import { useGLTF } from '@react-three/drei';

export function Teapot() {
  const { nodes, materials } = useGLTF('https://market-assets.fra1.cdn.digitaloceanspaces.com/market-assets/models/utah-teapot/model.gltf');
  
  return (
    <group rotation={[-Math.PI / 2, 0, 0]} scale={0.5}>
      <mesh
        geometry={nodes.Teapot.geometry}
        material={materials['Default Material']}
        material-color="#61dafb"
        material-roughness={0.3}
        material-metalness={0.8}
      />
    </group>
  );
} 