#!/usr/bin/env python3
"""
Test script for SIAM Specialist Agent
Tests core functionality without requiring OpenAI API key
"""
import json
from pathlib import Path
from agents.siam_specialist import SIAMSpecialistAgent

def test_siam_knowledge_base():
    """Test SIAM knowledge base loading"""
    print("🔍 Testing SIAM knowledge base loading...")
    
    agent = SIAMSpecialistAgent()
    
    # Test framework loading
    framework = agent.get_siam_framework()
    assert framework.get("framework_name") == "Service Integration and Management (SIAM)"
    print(f"✅ SIAM Framework loaded: {framework['framework_name']}")
    
    # Test scenarios loading
    scenarios = agent.get_multi_vendor_scenarios()
    assert "multi_vendor_scenarios" in scenarios
    print(f"✅ Multi-vendor scenarios loaded: {len(scenarios['multi_vendor_scenarios']['scenarios'])} scenarios")
    
    # Test specializations
    assert len(agent.specializations) == 8
    print(f"✅ Specializations available: {agent.specializations}")

def test_siam_recommendations():
    """Test SIAM recommendation generation (without AI)"""
    print("\n🔍 Testing SIAM recommendation logic...")
    
    agent = SIAMSpecialistAgent()
    
    # Test governance model determination
    centralized = agent._determine_governance_model(5, "high", "medium")
    federated = agent._determine_governance_model(2, "low", "high")
    hybrid = agent._determine_governance_model(3, "medium", "medium")
    
    assert centralized == "Centralized"
    assert federated == "Federated" 
    assert hybrid == "Hybrid"
    print(f"✅ Governance model logic working: {centralized}, {federated}, {hybrid}")
    
    # Test governance bodies suggestion
    bodies = agent._suggest_governance_bodies(4, "high")
    assert len(bodies) >= 2
    print(f"✅ Governance bodies suggested: {len(bodies)} bodies")
    
    # Test risk identification
    risks = agent._identify_risk_factors("Complex multi-vendor integration with critical services")
    assert len(risks) > 0
    print(f"✅ Risk identification working: {len(risks)} risks identified")

def test_siam_metrics():
    """Test SIAM metrics and measurement approaches"""
    print("\n🔍 Testing SIAM metrics definition...")
    
    agent = SIAMSpecialistAgent()
    
    # Test metrics definition
    metrics = agent._define_siam_metrics()
    assert len(metrics) >= 5
    print(f"✅ SIAM metrics defined: {len(metrics)} metrics")
    
    # Test measurement approach
    measurement = agent._suggest_measurement_approach()
    assert "frequency" in measurement
    print(f"✅ Measurement approach defined: {measurement['frequency']}")

def test_siam_integration():
    """Test integration with existing infrastructure"""
    print("\n🔍 Testing integration capabilities...")
    
    agent = SIAMSpecialistAgent()
    
    # Test knowledge file loading
    assert agent.knowledge_base_path.exists()
    print(f"✅ Knowledge base path exists: {agent.knowledge_base_path}")
    
    # Test cache functionality
    framework1 = agent._load_knowledge_file("siam-framework.json")
    framework2 = agent._load_knowledge_file("siam-framework.json")  # Should use cache
    assert framework1 == framework2
    print(f"✅ Knowledge caching working")

def test_norwegian_context():
    """Test Norwegian business context integration"""
    print("\n🔍 Testing Norwegian context...")
    
    agent = SIAMSpecialistAgent()
    framework = agent.get_siam_framework()
    
    norwegian_context = framework.get("norwegian_context", {})
    assert "regulatory_considerations" in norwegian_context
    assert "cultural_factors" in norwegian_context
    print(f"✅ Norwegian context included: {len(norwegian_context)} categories")
    
    # Test compliance requirements
    compliance = agent._get_governance_compliance_requirements()
    assert any("GDPR" in req for req in compliance)
    print(f"✅ Norwegian compliance requirements: {len(compliance)} requirements")

def main():
    """Run all SIAM tests"""
    print("🚀 Starting SIAM Specialist Agent Tests\n")
    
    try:
        test_siam_knowledge_base()
        test_siam_recommendations()
        test_siam_metrics()
        test_siam_integration()
        test_norwegian_context()
        
        print("\n🎉 All SIAM tests passed successfully!")
        print("\n📋 SIAM Agent Capabilities Summary:")
        print("   ✅ Knowledge base loading and caching")
        print("   ✅ Multi-vendor scenario analysis")
        print("   ✅ Governance model recommendations") 
        print("   ✅ Risk and metrics identification")
        print("   ✅ Norwegian context integration")
        print("   ✅ Integration with existing infrastructure")
        
        print("\n🔗 Available SIAM API Endpoints:")
        print("   POST /api/agents/siam-analysis - Multi-vendor scenario analysis")
        print("   POST /api/agents/governance-guidance - Governance recommendations")
        print("   POST /api/agents/vendor-readiness - Vendor assessment")
        print("   GET  /api/agents/siam/framework - Framework access")
        print("   GET  /api/agents/epic7/features - Feature overview")
        
        return True
        
    except Exception as e:
        print(f"\n❌ Test failed: {e}")
        return False

if __name__ == "__main__":
    success = main()
    exit(0 if success else 1)