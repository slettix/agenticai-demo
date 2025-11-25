import React from 'react';
import './ForsvaretLogo.css';
import logoSvg from '../../assets/logo.svg';

interface ForsvaretLogoProps {
  size?: 'small' | 'medium' | 'large';
  className?: string;
}

export const ForsvaretLogo: React.FC<ForsvaretLogoProps> = ({ 
  size = 'medium', 
  className = '' 
}) => {
  const sizeMap = {
    small: 32,
    medium: 48,
    large: 64
  };
  
  const logoSize = sizeMap[size];
  const logoHeight = Math.round(logoSize * 1.234); // Maintaining 256:316 aspect ratio

  return (
    <div className={`forsvaret-logo ${size} ${className}`}>
      <img 
        src={logoSvg}
        alt="Forsvaret logo"
        width={logoSize}
        height={logoHeight}
        style={{
          width: logoSize,
          height: logoHeight,
          objectFit: 'contain'
        }}
      />
    </div>
  );
};

export default ForsvaretLogo;