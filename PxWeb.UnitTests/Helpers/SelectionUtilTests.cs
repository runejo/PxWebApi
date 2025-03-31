﻿using PxWeb.Helper.Api2;

namespace PxWeb.UnitTests.Helpers
{
    [TestClass]
    public class SelectionUtilTests
    {
        [TestMethod]
        public void StubOrHeading_OneVariableHasMoreValues_ReturnsVariableWithMoreValuesAsStub()
        {
            // Arrange
            var variableWith5Values = ModelStore.CreateClassificationVariable("A", PlacementType.Stub, 5);
            var variableWith10Values = ModelStore.CreateClassificationVariable("A", PlacementType.Stub, 10);

            // Act
            var (stub, heading) = SelectionUtil.StubOrHeading(variableWith5Values, variableWith10Values);
            // Assert
            Assert.AreEqual(variableWith5Values, heading);
            Assert.AreEqual(variableWith10Values, stub);
        }

        [TestMethod]
        public void StubOrHeading_OneVariableHasMoreValuesInDiffrentOrder_ReturnsVariableWithMoreValuesAsStub()
        {
            // Arrange
            var variableWith5Values = ModelStore.CreateClassificationVariable("A", PlacementType.Stub, 5);
            var variableWith10Values = ModelStore.CreateClassificationVariable("A", PlacementType.Stub, 10);

            // Act
            var (stub, heading) = SelectionUtil.StubOrHeading(variableWith10Values, variableWith5Values);
            // Assert
            Assert.AreEqual(variableWith5Values, heading);
            Assert.AreEqual(variableWith10Values, stub);
        }

        [TestMethod]
        public void IsMandatory_OptionalVariable_ReturnsFalse()
        {
            // Arrange
            var model = ModelStore.CreateModelA();
            var selection = new VariableSelection() { VariableCode = "GENDER" };

            //Act
            var mandatory = SelectionUtil.IsMandatory(model, selection);

            // Assert
            Assert.IsFalse(mandatory);

        }

        [TestMethod]
        public void IsMandatory_MandatoryVariable_ReturnsTrue()
        {
            // Arrange
            var model = ModelStore.CreateModelA();
            var selection = new VariableSelection() { VariableCode = "MEASURE" };

            //Act
            var mandatory = SelectionUtil.IsMandatory(model, selection);

            // Assert
            Assert.IsTrue(mandatory);

        }

        [TestMethod]
        public void UseDefaultSelection_Null_ReturnsTrue()
        {
            //Act
            var useDefaultSelection = SelectionUtil.UseDefaultSelection(null);

            // Assert
            Assert.IsTrue(useDefaultSelection);

        }

        [TestMethod]
        public void UseDefaultSelection_EmptySelection_ReturnsTrue()
        {
            // Arrange
            var selection = SelectionUtil.CreateEmptyVariablesSelection();

            //Act
            var useDefaultSelection = SelectionUtil.UseDefaultSelection(selection);

            // Assert
            Assert.IsTrue(useDefaultSelection);

        }

        [TestMethod]
        public void UseDefaultSelection_OneSelectionsDefined_ReturnsFalse()
        {
            // Arrange
            var selection = SelectionUtil.CreateEmptyVariablesSelection();
            selection.Selection.Add(new VariableSelection() { VariableCode = "Test", ValueCodes = new List<string>() { "*" } });

            //Act
            var useDefaultSelection = SelectionUtil.UseDefaultSelection(selection);

            // Assert
            Assert.IsFalse(useDefaultSelection);

        }
    }
}
