﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Core;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [BlockProperty( 0, "Entity", "Applies To", "Entity Name", false, "" )]
    [BlockProperty( 1, "Entity Qualifier Column", "Applies To", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
    [BlockProperty( 2, "Entity Qualifier Value", "Applies To", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
    [BlockProperty( 3, "Allow Setting of Values", "SetValues", "Set Values", "Should UI be available for setting values of the specified Entity ID?", false, "false", "Rock", "Rock.Field.Types.Boolean" )]
    [BlockProperty( 4, "Entity Id", "Set Values", "The entity id that values apply to", false, "" )]
    public partial class Attributes : RockBlock
    {
        #region Fields

        protected string _entity = string.Empty;
        protected string _entityQualifierColumn = string.Empty;
        protected string _entityQualifierValue = string.Empty;
        protected bool _setValues = false;
        protected int? _entityId = null;

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _entity = AttributeValue( "Entity" );
            if ( string.IsNullOrWhiteSpace( _entity ) )
                _entity = PageParameter( "Entity" );

            _entityQualifierColumn = AttributeValue( "EntityQualifierColumn" );
            if ( string.IsNullOrWhiteSpace( _entityQualifierColumn ) )
                _entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

            _entityQualifierValue = AttributeValue( "EntityQualifierValue" );
            if ( string.IsNullOrWhiteSpace( _entityQualifierValue ) )
                _entityQualifierValue = PageParameter( "EntityQualifierValue" );

            _setValues = Convert.ToBoolean( AttributeValue( "SetValues" ) );

            string entityIdString = AttributeValue( "EntityId" );
            if ( string.IsNullOrWhiteSpace( entityIdString ) )
                entityIdString = PageParameter( "EntityId" );
            if ( !string.IsNullOrWhiteSpace( entityIdString ) )
            {
                int entityIdint = 0;
                if ( Int32.TryParse( entityIdString, out entityIdint ) )
                    _entityId = entityIdint;
            }

            _canConfigure = CurrentPage.IsAuthorized( "Configure", CurrentPerson );

            BindFilter();

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.Actions.IsAddEnabled = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                rGrid.Columns[7].Visible = !_setValues;
                rGrid.Columns[8].Visible = _setValues;
                rGrid.Columns[10].Visible = _setValues;

                modalDetails.SaveClick += modalDetails_SaveClick;
                modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValues.ClientID );
                
                // Create the dropdown list for listing the available field types
                var fieldTypeService = new Rock.Core.FieldTypeService();
                var items = fieldTypeService.
                    Queryable().
                    Select( f => new { f.Id, f.Name } ).
                    OrderBy( f => f.Name );

                ddlFieldType.AutoPostBack = true;
                ddlFieldType.SelectedIndexChanged += new EventHandler( ddlFieldType_SelectedIndexChanged );
                ddlFieldType.Items.Clear();
                foreach ( var item in items )
                    ddlFieldType.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );

                string editAttributeId = Request.Form[hfIdValues.UniqueID];
                if ( Page.IsPostBack && editAttributeId != null && editAttributeId.Trim() != string.Empty )
                    ShowEditValue( Int32.Parse( editAttributeId ), false );
            }
            else
            {
                nbMessage.Text = "You are not authorized to configure this page";
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void modalDetails_SaveClick( object sender, EventArgs e )
        {
            if ( _setValues )
            {
                int attributeId = 0;
                if ( hfIdValues.Value != string.Empty && !Int32.TryParse( hfIdValues.Value, out attributeId ) )
                    attributeId = 0;

                if ( attributeId != 0 && phEditControl.Controls.Count > 0 )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );

                    AttributeValueService attributeValueService = new AttributeValueService();
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();
                    if ( attributeValue == null )
                    {
                        attributeValue = new Rock.Core.AttributeValue();
                        attributeValue.AttributeId = attributeId;
                        attributeValue.EntityId = _entityId;
                        attributeValueService.Add( attributeValue, CurrentPersonId );
                    }

                    var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );
                    attributeValue.Value = fieldType.Field.GetEditValue( phEditControl.Controls[0], attribute.QualifierValues );

                    attributeValueService.Save( attributeValue, CurrentPersonId );

                    Rock.Web.Cache.AttributeCache.Flush( attributeId );
                    if ( _entity == string.Empty && _entityQualifierColumn == string.Empty && _entityQualifierValue == string.Empty && !_entityId.HasValue )
                        Rock.Web.Cache.GlobalAttributesCache.Flush();

                }

                modalDetails.Hide();
            }

            BindGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
                BindGrid();

            if ( Page.IsPostBack && hfId.Value != string.Empty )
                BuildConfigControls();

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCategoryFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)rGrid.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_EditValue( object sender, RowEventArgs e )
        {
            if ( _setValues )
                ShowEditValue( (int)rGrid.DataKeys[e.RowIndex]["id"], true );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var attributeService = new Rock.Core.AttributeService();

            Rock.Core.Attribute attribute = attributeService.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );
            if ( attribute != null )
            {
                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );

                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int attributeId = (int)rGrid.DataKeys[e.Row.RowIndex].Value;

                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldTypeId );

                if ( _setValues )
                {
                    Literal lValue = e.Row.FindControl( "lValue" ) as Literal;
                    if ( lValue != null )
                    {
                        AttributeValueService attributeValueService = new AttributeValueService();
                        var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();
                        if ( attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) )
                            lValue.Text = fieldType.Field.FormatValue( lValue, attributeValue.Value, attribute.QualifierValues, true );
                        else
                            lValue.Text = string.Format( "<span class='muted'>{0}</span>", fieldType.Field.FormatValue( lValue, attribute.DefaultValue, attribute.QualifierValues, true ) );
                    }
                }
                else
                {
                    Literal lDefaultValue = e.Row.FindControl( "lDefaultValue" ) as Literal;
                    if ( lDefaultValue != null )
                        lDefaultValue.Text = fieldType.Field.FormatValue( lDefaultValue, attribute.DefaultValue, attribute.QualifierValues, true );
                }

            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFieldType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void ddlFieldType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int attributeId = 0;
            if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out attributeId ) )
                attributeId = 0;

            if ( attributeId != 0 )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                BuildConfigControls( attribute.QualifierValues );
            }
            else
                BuildConfigControls();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var attributeService = new Rock.Core.AttributeService();
                var attributeQualifierService = new Rock.Core.AttributeQualifierService();

                Rock.Core.Attribute attribute;

                int attributeId = 0;
                if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out attributeId ) )
                    attributeId = 0;

                if ( attributeId == 0 )
                {
                    attribute = new Rock.Core.Attribute();
                    attribute.IsSystem = false;
                    attribute.Entity = _entity;
                    attribute.EntityQualifierColumn = _entityQualifierColumn;
                    attribute.EntityQualifierValue = _entityQualifierValue;
                    attributeService.Add( attribute, CurrentPersonId );
                }
                else
                {
                    Rock.Web.Cache.AttributeCache.Flush( attributeId );
                    attribute = attributeService.Get( attributeId );
                }

                attribute.Key = tbKey.Text;
                attribute.Name = tbName.Text;
                attribute.Category = tbCategory.Text;
                attribute.Description = tbDescription.Text;
                attribute.FieldTypeId = Int32.Parse( ddlFieldType.SelectedValue );

                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldTypeId );

                foreach ( var oldQualifier in attribute.AttributeQualifiers.ToList() )
                    attributeQualifierService.Delete( oldQualifier, CurrentPersonId );
                attribute.AttributeQualifiers.Clear();

                List<Control> configControls = new List<Control>();
                foreach ( var key in fieldType.Field.ConfigurationKeys() )
                    configControls.Add( phFieldTypeQualifiers.FindControl( "configControl_" + key ) );
                foreach ( var configValue in fieldType.Field.ConfigurationValues( configControls ) )
                {
                    AttributeQualifier qualifier = new AttributeQualifier();
                    qualifier.IsSystem = false;
                    qualifier.Key = configValue.Key;
                    qualifier.Value = configValue.Value.Value ?? string.Empty;
                    attribute.AttributeQualifiers.Add( qualifier );
                }

                attribute.DefaultValue = tbDefaultValue.Text;
                attribute.IsMultiValue = cbMultiValue.Checked;
                attribute.IsRequired = cbRequired.Checked;

                attributeService.Save( attribute, CurrentPersonId );
            }

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( "[All]" );

            AttributeService attributeService = new AttributeService();
            var items = attributeService.Queryable().
                Where( a => a.Entity == _entity &&
                    ( a.EntityQualifierColumn ?? string.Empty ) == _entityQualifierColumn &&
                    ( a.EntityQualifierValue ?? string.Empty ) == _entityQualifierValue &&
                    a.Category != "" && a.Category != null ).
                OrderBy( a => a.Category ).
                Select( a => a.Category ).
                Distinct().ToList();

            foreach ( var item in items )
                ddlCategoryFilter.Items.Add( item );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var queryable = new Rock.Core.AttributeService().Queryable().
                Where( a => a.Entity == _entity &&
                    ( a.EntityQualifierColumn ?? string.Empty ) == _entityQualifierColumn &&
                    ( a.EntityQualifierValue ?? string.Empty ) == _entityQualifierValue );

            if ( ddlCategoryFilter.SelectedValue != "[All]" )
                queryable = queryable.
                    Where( a => a.Category == ddlCategoryFilter.SelectedValue );

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
                queryable = queryable.
                    Sort( sortProperty );
            else
                queryable = queryable.
                    OrderBy( a => a.Category ).
                    ThenBy( a => a.Key );

            rGrid.DataSource = queryable.ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int attributeId )
        {
            var attributeModel = new Rock.Core.AttributeService().Get( attributeId );

            if ( attributeModel != null )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeModel );

                lAction.Text = "Edit";
                hfId.Value = attribute.Id.ToString();

                tbKey.Text = attribute.Key;
                tbName.Text = attribute.Name;
                tbCategory.Text = attribute.Category;
                tbDescription.Text = attribute.Description;
                ddlFieldType.SelectedValue = attribute.FieldType.Id.ToString();
                BuildConfigControls( attribute.QualifierValues );
                tbDefaultValue.Text = attribute.DefaultValue;
                cbMultiValue.Checked = attribute.IsMultiValue;
                cbRequired.Checked = attribute.IsRequired;
            }
            else
            {
                lAction.Text = "Add";
                hfId.Value = string.Empty;

                tbKey.Text = string.Empty;
                tbName.Text = string.Empty;
                tbCategory.Text = ddlCategoryFilter.SelectedValue != "[All]" ? ddlCategoryFilter.SelectedValue : string.Empty;
                tbDescription.Text = string.Empty;

                FieldTypeService fieldTypeService = new FieldTypeService();
                var fieldTypeModel = fieldTypeService.GetByName( "Text" ).FirstOrDefault();
                if ( fieldTypeModel != null )
                    ddlFieldType.SelectedValue = fieldTypeModel.Id.ToString();
                BuildConfigControls();

                tbDefaultValue.Text = string.Empty;
                cbMultiValue.Checked = false;
                cbRequired.Checked = false;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Builds the config controls.
        /// </summary>
        private void BuildConfigControls()
        {
            BuildConfigControls( null );
        }

        /// <summary>
        /// Builds the config controls.
        /// </summary>
        /// <param name="qualifierValues">The qualifier values.</param>
        private void BuildConfigControls( Dictionary<string, Rock.Field.ConfigurationValue> qualifierValues )
        {
            var fieldType = Rock.Web.Cache.FieldTypeCache.Read( Int32.Parse( ddlFieldType.SelectedValue ) );
            if ( fieldType != null )
            {
                phFieldTypeQualifiers.Controls.Clear();
                var configControls = fieldType.Field.ConfigurationControls();

                int i = 0;
                foreach ( var configValue in fieldType.Field.ConfigurationValues( null ) )
                {
                    var ctrlGroup = new HtmlGenericControl( "div" );
                    phFieldTypeQualifiers.Controls.Add( ctrlGroup );
                    ctrlGroup.AddCssClass( "control-group" );

                    var lbl = new Label();
                    ctrlGroup.Controls.Add( lbl );
                    lbl.AddCssClass( "control-label" );
                    lbl.Text = configValue.Value.Name;

                    var ctrls = new HtmlGenericControl( "div" );
                    ctrlGroup.Controls.Add( ctrls );
                    ctrls.AddCssClass( "controls" );

                    Control control = configControls[i];
                    ctrls.Controls.Add( control );
                    control.ID = "configControl_" + configValue.Key;

                    i++;
                }

                if ( qualifierValues != null )
                    fieldType.Field.SetConfigurationValues( configControls, qualifierValues );
            }
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( int attributeId, bool setValues )
        {
            if ( _setValues )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );

                hfIdValues.Value = attribute.Id.ToString();
                lCaption.Text = attribute.Name;

                AttributeValueService attributeValueService = new AttributeValueService();
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();

                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );

                Control editControl = fieldType.Field.EditControl( attribute.QualifierValues );
                if ( setValues && attributeValue != null )
                    fieldType.Field.SetEditValue( editControl, attribute.QualifierValues, attributeValue.Value );

                phEditControl.Controls.Clear();
                phEditControl.Controls.Add( editControl );

                modalDetails.Show();
            }
        }

        #endregion
    }
}