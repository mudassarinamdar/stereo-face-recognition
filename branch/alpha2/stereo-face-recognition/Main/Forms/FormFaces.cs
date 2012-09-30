﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FaceDetection.Misc;
using ImageFilters.Common;
using ImageFilters.Common.Models;

namespace FaceDetection.Forms
{
    public partial class FormFaces : Form
    {
        #region Fields

        private readonly FormMain _main;
        private bool _isRecording;

        #endregion

        #region Properties

        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;

                switch (_isRecording)
                {
                    case true:
                    {
                        buttonStartRecord.Enabled = false;
                        buttonEndRecord.Enabled = true;

                        tboxRecordNewLabel.Enabled = false;

                        buttonView.Enabled = false;
                        buttonDelete.Enabled = false;

                        buttonClose.Enabled = false;

                        dgvFaces.Enabled = false;

                        break;
                    }
                    case false:
                    {
                        buttonStartRecord.Enabled = true;
                        buttonEndRecord.Enabled = false;

                        tboxRecordNewLabel.Enabled = true;

                        buttonView.Enabled = true;
                        buttonDelete.Enabled = true;

                        buttonClose.Enabled = true;

                        dgvFaces.Enabled = true;

                        break;
                    }
                }
            }
        }

        public bool IsTraining { get; set; }

        public FaceModel TrainingFaceModel { get; set; }

        #endregion

        #region Methods

        private void OnFormFacesLoad(object sender, EventArgs e)
        {
            dgvFaces.DataSource = HelperFaces.Faces;
        }

        private void OnButtonStartRecordClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tboxRecordNewLabel.Text))
            {
                return;
            }

            TrainingFaceModel = HelperFaces.Faces.FirstOrDefault(item => item.Label == tboxRecordNewLabel.Text);

            if (TrainingFaceModel == null)
            {
                TrainingFaceModel = new FaceModel(tboxRecordNewLabel.Text);
                HelperFaces.Faces.Add(TrainingFaceModel);
            }

            IsRecording = true;

            _main.FacesAvailable += OnMainFacesAvailable;
        }

        private void OnButtonEndRecordClick(object sender, EventArgs e)
        {
            _main.FacesAvailable -= OnMainFacesAvailable;

            IsRecording = false;

            IsTraining = true;

            try
            {
                HelperFaces.TrainRecognizer();
            }
            finally
            {
                IsTraining = false;

                dgvFaces.DataSource = null;
                dgvFaces.DataSource = HelperFaces.Faces;
            }
        }

        private void OnButtonCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OnMainFacesAvailable(object sender, FaceRegionsEventArgs e)
        {
            if (IsRecording == false)
            {
                return;
            }

            var faceRegions = new List<FaceRegion2D>();

            #region Add Face Regions

            if (e.Left != null)
            {
                faceRegions.AddRange(e.Left);
            }

            if (e.Right != null)
            {
                faceRegions.AddRange(e.Right);
            }

            #endregion

            TrainingFaceModel.Images.AddRange(faceRegions.
                                                  Where(faceRegion => faceRegion.LeftEye != null && faceRegion.RightEye != null).
                                                  Select(faceRegion => faceRegion.FaceImage));

            Invoke(new Action(delegate
                              {
                                  dgvFaces.DataSource = null;
                                  dgvFaces.DataSource = HelperFaces.Faces;
                              }));
        }

        #endregion

        #region Instance

        public FormFaces(FormMain main)
        {
            InitializeComponent();

            _main = main;

            IsRecording = false;
        }

        #endregion
    }
}