﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xamarin.Essentials;

namespace BandyFW
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);
		}

		protected override void OnResume()
		{
			base.OnResume();
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
			ISharedPreferencesEditor editor = prefs.Edit();

			Button submit_button = FindViewById<Button>(Resource.Id.submit_button);
			Button remember_button = FindViewById<Button>(Resource.Id.remember_button);
			Button restore_button = FindViewById<Button>(Resource.Id.restore_button);
			Button reset_button = FindViewById<Button>(Resource.Id.reset_button);
			Button clear_response_button = FindViewById<Button>(Resource.Id.clear_response_button);
			EditText model_text = FindViewById<EditText>(Resource.Id.model_text);
			EditText production_text = FindViewById<EditText>(Resource.Id.production_text);

			var app_radio = FindViewById<RadioGroup>(Resource.Id.app_radio);
			RadioButton radio_zepp = FindViewById<RadioButton>(Resource.Id.radio_zepp);
			RadioButton radio_mifit = FindViewById<RadioButton>(Resource.Id.radio_mifit);

			CheckBox play_postfix_checkbox = FindViewById<CheckBox>(Resource.Id.play_postfix_checkbox);

			EditText app_name_text = FindViewById<EditText>(Resource.Id.app_name_text);
			EditText app_version_number_text = FindViewById<EditText>(Resource.Id.app_version_number_text);
			EditText app_version_build_text = FindViewById<EditText>(Resource.Id.app_version_build_text);
			EditText response_text = FindViewById<EditText>(Resource.Id.response_text);

			Spinner spinner = FindViewById<Spinner>(Resource.Id.device_model_spinner);
			var items = new List<string>() {
				GetString(Resource.String.device_name_chaohu),
				GetString(Resource.String.device_name_cinco_nfc),
				GetString(Resource.String.device_name_pyh),
				GetString(Resource.String.device_name_kongming_china_nfc),
				GetString(Resource.String.device_name_cinco_global),
				GetString(Resource.String.device_name_chaohulite),
				GetString(Resource.String.device_name_kongming_china)
			};
			var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, items);
			spinner.Adapter = adapter;
			spinner.ItemSelected += spinner_ItemSelected;

			app_name_text.Focusable = false;

			bool radio_zepp_bool = false;
			bool radio_mifit_bool = false;

			string zepp_name = "com.huami.midong";
			string mifit_name = "com.xiaomi.hm.health";

			app_radio.CheckedChange += (s, e) =>
			{
				if (radio_zepp.Checked)
				{
					app_name_text.Text = zepp_name;
					radio_zepp_bool = true;
					radio_mifit_bool = false;
					play_postfix_checkbox.Enabled = true;

				}
				if (radio_mifit.Checked)
				{
					app_name_text.Text = mifit_name;
					radio_zepp_bool = false;
					radio_mifit_bool = true;
					play_postfix_checkbox.Enabled = false;
				}
			};

			var play_postfix = "";
			bool play_postfix_bool = false;
			play_postfix_checkbox.CheckedChange += (s, e) =>
			{
				if (play_postfix_checkbox.Checked)
				{
					play_postfix = "-play";
					play_postfix_bool = true;

				}
				else
				{
					play_postfix = "";
					play_postfix_bool = false;
				}
			};

			//Submit button logics (Get request + change request field)
			submit_button.Click += async delegate
			{
				var current = Connectivity.NetworkAccess;
				if (current == Xamarin.Essentials.NetworkAccess.Internet)
				{
					if (model_text.Text == "" || production_text.Text == "" || app_name_text.Text == GetString(Resource.String.shoose_app) || app_version_number_text.Text == "" || app_version_build_text.Text == "")
					{
						Toast.MakeText(this, Resource.String.input_all_values, ToastLength.Long).Show();
					}
					else
					{
						HttpClient client = new HttpClient();
						HttpRequestMessage request = new HttpRequestMessage();

						string request_host = "api-mifit-ru.huami.com";

						UriBuilder uriBuilder = new UriBuilder
						{
							Scheme = "https",
							Host = request_host,
							Path = "devices/ALL/hasNewVersion",
							Query = "productId=0&vendorSource=0&resourceVersion=0&firmwareFlag=0&vendorId=0&resourceFlag=0&productionSource=" + production_text.Text + "&userid=0&userId=0&deviceSource=" + model_text.Text + "&fontVersion=0&fontFlag=0&appVersion=" + app_version_number_text.Text + play_postfix + "_" + app_version_build_text.Text + "&appid=0&callid=0&channel=0&country=0&cv=0&device=0&deviceType=ALL&device_type=0&firmwareVersion=0&hardwareVersion=0&lang=0&support8Bytes=true&timezone=0&v=0",
						};
						Uri URL = uriBuilder.Uri;
						String stringUri;
						stringUri = URL.ToString();

						request.RequestUri = new Uri(stringUri);
						request.Method = HttpMethod.Get;

						request.Headers.Add("hm-privacy-diagnostics", "0");
						request.Headers.Add("country", "0");
						request.Headers.Add("appplatform", "android_phone");
						request.Headers.Add("hm-privacy-ceip", "0");
						request.Headers.Add("X-Request-Id", "0");
						request.Headers.Add("timezone", "0");
						request.Headers.Add("channel", "0");
						request.Headers.Add("User-Agent", "0");
						request.Headers.Add("cv", "0");
						request.Headers.Add("appname", app_name_text.Text);
						request.Headers.Add("v", "0");
						request.Headers.Add("apptoken", "0");
						request.Headers.Add("lang", "0");
						request.Headers.Add("Host", "api-mifit-ru.huami.com");
						request.Headers.Add("Connection", "0");
						request.Headers.Add("Accept-Encoding", "0");

						HttpResponseMessage response = await client.SendAsync(request);
						if (response.StatusCode == HttpStatusCode.OK)
						{
							HttpContent responseContent = response.Content;

							var server_response = await responseContent.ReadAsStringAsync();
							response_text.Text = server_response;

							//For firmware_json.cs
							//var data = JsonConvert.DeserializeObject<Firmware_json>(json);
						}
					}
				}
				else
				{
					Toast.MakeText(this, "Enable network!", ToastLength.Short).Show();
				}
			};

			//Remember button logics
			remember_button.Click += delegate
			{
				if (model_text.Text == "" || production_text.Text == "" || app_name_text.Text == GetString(Resource.String.shoose_app) || app_version_number_text.Text == "" || app_version_build_text.Text == "" || response_text.Text == "")
				{
					RunOnUiThread(() => Toast.MakeText(this, Resource.String.no_values_for_saving, ToastLength.Short).Show());
				}
				else
				{
					editor.PutString("model_code", model_text.Text);
					editor.PutString("production_code", production_text.Text);
					editor.PutString("app_name", app_name_text.Text);
					editor.PutBoolean("radio_zepp_bool", radio_zepp_bool);
					editor.PutBoolean("radio_mifit_bool", radio_mifit_bool);
					editor.PutBoolean("play_postfix_bool", play_postfix_bool);
					editor.PutString("app_version_number", app_version_number_text.Text);
					editor.PutString("app_version_build", app_version_build_text.Text);
					editor.PutString("response", response_text.Text);
					editor.Apply();
					RunOnUiThread(() => Toast.MakeText(this, Resource.String.remember_success, ToastLength.Short).Show());
				}
			};

			//Restore button logics
			restore_button.Click += delegate
			{
				if (prefs.GetString("model_code", null) == "" && prefs.GetString("production_code", null) == "" && prefs.GetString("app_name", null) == "" && prefs.GetString("app_version", null) == "" && prefs.GetString("response", null) == "")
				{
					RunOnUiThread(() => Toast.MakeText(this, Resource.String.no_values_in_memory, ToastLength.Short).Show());
				}
				else
				{
					model_text.Text = prefs.GetString("model_code", null);
					production_text.Text = prefs.GetString("production_code", null);
					app_name_text.Text = prefs.GetString("app_name", null);
					radio_zepp_bool = prefs.GetBoolean("radio_zepp_bool", false);
					radio_mifit_bool = prefs.GetBoolean("radio_mifit_bool", false);
					play_postfix_bool = prefs.GetBoolean("play_postfix_bool", false);

					if (radio_zepp_bool == true)
					{
						radio_zepp.Checked = true;
					}
					else
					{
						radio_zepp.Checked = false;
					}

					if (radio_mifit_bool == true)
					{
						radio_mifit.Checked = true;
					}
					else
					{
						radio_mifit.Checked = false;
					}

					if (play_postfix_bool == true)
					{
						play_postfix_checkbox.Checked = true;
					}
					else
					{
						play_postfix_checkbox.Checked = false;
					}

					app_version_number_text.Text = prefs.GetString("app_version_number", null);
					app_version_build_text.Text = prefs.GetString("app_version_build", null);
					response_text.Text = prefs.GetString("response", null);
					RunOnUiThread(() => Toast.MakeText(this, "Restore success", ToastLength.Short).Show());
				}
			};

			//Reset button logics
			reset_button.Click += delegate
			{
				model_text.Text = "";
				production_text.Text = "";
				app_name_text.Text = GetString(Resource.String.shoose_app);
				app_version_number_text.Text = "";
				response_text.Text = "";

				editor.PutString("model_code", model_text.Text);
				editor.PutString("production_code", production_text.Text);
				editor.PutString("app_name", app_name_text.Text);
				editor.PutBoolean("radio_zepp_bool", radio_zepp_bool);
				editor.PutBoolean("radio_mifit_bool", radio_mifit_bool);
				editor.PutBoolean("play_postfix_bool", play_postfix_bool);
				editor.PutString("app_version_number", app_version_number_text.Text);
				editor.PutString("app_version_build", app_version_build_text.Text);
				editor.PutString("response", response_text.Text);
				editor.Apply();

				RunOnUiThread(() => Toast.MakeText(this, Resource.String.reset_success, ToastLength.Short).Show());
			};
			clear_response_button.Click += delegate
			{
				response_text.Text = "";
			};

			//App name field click logics
			app_name_text.Click += delegate
			{
				if (radio_zepp.Checked || radio_mifit.Checked)
				{
					RunOnUiThread(() => Toast.MakeText(this, app_name_text.Text, ToastLength.Short).Show());
				}
				else
				{
					app_name_text.SetText(Resource.String.shoose_app);
				}
			};
		}


		public void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			EditText model_text = FindViewById<EditText>(Resource.Id.model_text);
			EditText production_text = FindViewById<EditText>(Resource.Id.production_text);
			EditText app_name_text = FindViewById<EditText>(Resource.Id.app_name_text);
			RadioButton radio_zepp = FindViewById<RadioButton>(Resource.Id.radio_zepp);
			RadioButton radio_mifit = FindViewById<RadioButton>(Resource.Id.radio_mifit);

			Spinner spinner = (Spinner)sender;
			if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_chaohu))
			{
				model_text.Text = "12";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_cinco_nfc))
			{
				model_text.Text = "24";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_cinco_global))
			{
				model_text.Text = "30";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_kongming_china_nfc))
			{
				model_text.Text = "58";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_cinco_global))
			{
				model_text.Text = "25";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_chaohulite))
			{
				model_text.Text = "42";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_kongming_china))
			{
				model_text.Text = "59";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
		}
	}
}
